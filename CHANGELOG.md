# Changelog

## 1.3.0 — prepared, not yet published

This release line includes the previously unreleased nested-property count and enumeration fixes already on `master`, plus the following changes:

- Reject negative indices consistently for single-property, array-backed, empty and chained logger property lists (consolidated from PR #11).
- Pass the logging provider's exception argument to the original formatter instead of always reusing the exception captured during `Log`.
- Annotate nullable property values and document the existing public overloads without removing them.
- Preserve the `netstandard2.0` asset and the logging-abstractions dependency minimum; test with modern .NET 8/9/10 logging implementations on Linux, Windows and macOS.
- Replace obsolete build versioning with an explicit 1.3.0 version and standard .NET 10 SDK commands.
- Use xUnit.net v3 and Microsoft.Testing.Platform, retain regression coverage from PR #11, and add logging, formatter, scope, overload and null-value tests.
- Validate public API compatibility against 1.2.7 and run the full suite against the packed DLL.
- Include a NuGet README, license, XML API docs, repository commit metadata, Source Link and portable symbols.
- Replace push-to-master API-key publication with explicit versioned release tags, environment approval and NuGet Trusted Publishing. Pin GitHub Actions and add monthly dependency update PRs.

See `docs/RELEASING.md` for the one-time publisher setup. Issue #12 remains a publication request until the new package is actually available from nuget.org.

## 1.2.7 — 2020-09-29

Previous published version used as the API compatibility baseline.
