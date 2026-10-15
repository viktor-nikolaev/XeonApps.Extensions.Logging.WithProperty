"""Validate release metadata and the actual NuGet artifacts using only Python's standard library."""
import argparse
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET
from zipfile import ZipFile

PACKAGE = "XeonApps.Extensions.Logging.WithProperty"
ROOT = Path(__file__).resolve().parents[1]


def require(condition: bool, message: str) -> None:
    if not condition:
        raise ValueError(message)


def version() -> str:
    value = ET.parse(ROOT / "Directory.Build.props").findtext(".//Version", "")
    require(bool(re.fullmatch(r"(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(?:-[0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*)?", value)),
            "Version must be a three-part release or prerelease version")
    return value


def metadata(archive: ZipFile) -> ET.Element:
    names = [name for name in archive.namelist() if name.endswith(".nuspec")]
    require(len(names) == 1, "Expected exactly one nuspec")
    root = ET.fromstring(archive.read(names[0]))
    for node in root.iter():
        node.tag = node.tag.rsplit("}", 1)[-1]
    result = root.find("metadata")
    require(result is not None, "Package metadata is missing")
    return result


def verify(directory: Path, expected_commit: str | None) -> None:
    expected_version = version()
    package = directory / f"{PACKAGE}.{expected_version}.nupkg"
    symbols = directory / f"{PACKAGE}.{expected_version}.snupkg"
    require(list(directory.glob("*.nupkg")) == [package], "Expected exactly the versioned package")
    require(list(directory.glob("*.snupkg")) == [symbols], "Expected exactly the versioned symbol package")
    with ZipFile(package) as archive:
        meta = metadata(archive)
        require(meta.findtext("id") == PACKAGE, "Unexpected package ID")
        require(meta.findtext("version") == expected_version, "Unexpected package version")
        require(meta.findtext("readme") == "README.md", "Missing NuGet README metadata")
        license_node = meta.find("license")
        require(license_node is not None and license_node.get("type") == "expression"
                and license_node.text == "Apache-2.0", "Missing Apache-2.0 license expression")
        for name in ("README.md", "LICENSE", f"lib/netstandard2.0/{PACKAGE}.dll", f"lib/netstandard2.0/{PACKAGE}.xml"):
            require(name in archive.namelist() and len(archive.read(name)) > 0, f"Missing/empty {name}")
        assemblies = [name for name in archive.namelist() if name.startswith("lib/") and name.endswith(".dll")]
        require(assemblies == [f"lib/netstandard2.0/{PACKAGE}.dll"], "Unexpected target framework or assembly")
        dependencies = meta.findall(".//dependency")
        require(len(dependencies) == 1 and dependencies[0].get("id") == "Microsoft.Extensions.Logging.Abstractions",
                "Build/test dependencies must not leak into the package")
        require(dependencies[0].get("version") in ("1.0.0", "[1.0.0, )", "[1.0.0,)"),
                "The consumer dependency floor must remain 1.0.0")
        repository = meta.find("repository")
        require(repository is not None and repository.get("type") == "git", "Missing repository metadata")
        require(repository.get("url") == f"https://github.com/viktor-nikolaev/{PACKAGE}", "Unexpected repository URL")
        if expected_commit:
            require(repository.get("commit") == expected_commit, "Package source commit does not match checkout")
    with ZipFile(symbols) as archive:
        meta = metadata(archive)
        require(meta.findtext("id") == PACKAGE and meta.findtext("version") == expected_version,
                "Symbol identity does not match the package")
        pdb = f"lib/netstandard2.0/{PACKAGE}.pdb"
        require(pdb in archive.namelist(), "Missing symbol PDB")
        require(archive.read(pdb).startswith(b"BSJB"), "Symbols must use portable PDB format")
    print(f"Validated {package.name} and {symbols.name}")


def main() -> None:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("directory", type=Path, nargs="?")
    parser.add_argument("--commit")
    parser.add_argument("--tag", help="Check that an intended release tag matches the project version")
    args = parser.parse_args()
    if args.tag is not None:
        require(args.tag == f"v{version()}", f"Release tag must be v{version()}")
        print(f"Validated release tag {args.tag}")
    if args.directory is not None:
        verify(args.directory, args.commit)
    require(args.tag is not None or args.directory is not None, "Supply a package directory or --tag")


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, ET.ParseError) as error:
        print(f"Package validation failed: {error}", file=sys.stderr)
        sys.exit(1)
