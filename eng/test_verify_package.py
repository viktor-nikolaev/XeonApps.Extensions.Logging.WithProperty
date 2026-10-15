"""Regression tests for release guards; fixtures are synthetic ZIPs, not executable packages."""
import contextlib
import importlib.util
import io
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch
from zipfile import ZipFile

SPEC = importlib.util.spec_from_file_location("verify_package", Path(__file__).with_name("verify-package.py"))
verify_package = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(verify_package)


class ReleaseValidationTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.version_patch = patch.object(verify_package, "ROOT", self.root)
        self.version_patch.start()
        self.addCleanup(self.version_patch.stop)
        (self.root / "Directory.Build.props").write_text(
            "<Project><PropertyGroup><Version>1.3.0</Version></PropertyGroup></Project>", encoding="utf-8")
        package = verify_package.PACKAGE
        self.package_path = self.root / f"{package}.1.3.0.nupkg"
        self.symbol_path = self.root / f"{package}.1.3.0.snupkg"
        self.nuspec = f'''<package xmlns="http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"><metadata>
<id>{package}</id><version>1.3.0</version><readme>README.md</readme>
<license type="expression">Apache-2.0</license>
<repository type="git" url="https://github.com/viktor-nikolaev/{package}" commit="expected-commit" />
<dependencies><group targetFramework=".NETStandard2.0">
<dependency id="Microsoft.Extensions.Logging.Abstractions" version="1.0.0" />
</group></dependencies></metadata></package>'''
        self.files = {
            f"{package}.nuspec": self.nuspec,
            "README.md": "Readme", "LICENSE": "License",
            f"lib/netstandard2.0/{package}.dll": b"synthetic DLL fixture",
            f"lib/netstandard2.0/{package}.xml": "<doc />",
        }
        self.symbol_files = {
            f"{package}.nuspec": self.nuspec,
            f"lib/netstandard2.0/{package}.pdb": b"BSJBsynthetic portable PDB fixture",
        }

    def write_packages(self):
        for path, files in ((self.package_path, self.files), (self.symbol_path, self.symbol_files)):
            with ZipFile(path, "w") as archive:
                for name, contents in files.items():
                    archive.writestr(name, contents)

    def verify(self):
        self.write_packages()
        with contextlib.redirect_stdout(io.StringIO()):
            verify_package.verify(self.root, "expected-commit")

    def test_valid_metadata_and_symbols_are_accepted(self):
        self.verify()

    def test_wrong_version_is_rejected(self):
        self.files[f"{verify_package.PACKAGE}.nuspec"] = self.nuspec.replace("<version>1.3.0", "<version>1.3.1")
        with self.assertRaisesRegex(ValueError, "Unexpected package version"):
            self.verify()

    def test_wrong_source_commit_is_rejected(self):
        self.files[f"{verify_package.PACKAGE}.nuspec"] = self.nuspec.replace("expected-commit", "different-commit")
        with self.assertRaisesRegex(ValueError, "source commit"):
            self.verify()

    def test_missing_readme_is_rejected(self):
        del self.files["README.md"]
        with self.assertRaisesRegex(ValueError, "Missing/empty README"):
            self.verify()

    def test_raised_dependency_floor_is_rejected(self):
        self.files[f"{verify_package.PACKAGE}.nuspec"] = self.nuspec.replace('version="1.0.0"', 'version="10.0.0"')
        with self.assertRaisesRegex(ValueError, "dependency floor"):
            self.verify()

    def test_leaked_build_dependency_is_rejected(self):
        self.files[f"{verify_package.PACKAGE}.nuspec"] = self.nuspec.replace(
            "</group>", '<dependency id="xunit.v3.mtp-v2" version="4.0.2" /></group>')
        with self.assertRaisesRegex(ValueError, "Build/test dependencies"):
            self.verify()

    def test_unexpected_framework_assembly_is_rejected(self):
        self.files[f"lib/net10.0/{verify_package.PACKAGE}.dll"] = b"extra fixture"
        with self.assertRaisesRegex(ValueError, "Unexpected target framework"):
            self.verify()

    def test_nonportable_symbols_are_rejected(self):
        self.symbol_files[f"lib/netstandard2.0/{verify_package.PACKAGE}.pdb"] = b"wrong PDB format"
        with self.assertRaisesRegex(ValueError, "portable PDB"):
            self.verify()

    def test_mismatched_symbol_version_is_rejected(self):
        self.symbol_files[f"{verify_package.PACKAGE}.nuspec"] = self.nuspec.replace("<version>1.3.0", "<version>1.2.7")
        with self.assertRaisesRegex(ValueError, "Symbol identity"):
            self.verify()

    def test_missing_package_is_rejected(self):
        with self.assertRaisesRegex(ValueError, "versioned package"):
            verify_package.verify(self.root, None)

    def test_matching_release_tag_is_accepted(self):
        with patch("sys.argv", ["verify-package.py", "--tag", "v1.3.0"]), contextlib.redirect_stdout(io.StringIO()):
            verify_package.main()

    def test_mismatched_release_tag_is_rejected(self):
        with patch("sys.argv", ["verify-package.py", "--tag", "v1.2.7"]):
            with self.assertRaisesRegex(ValueError, "Release tag must be v1.3.0"):
                verify_package.main()


if __name__ == "__main__":
    unittest.main()
