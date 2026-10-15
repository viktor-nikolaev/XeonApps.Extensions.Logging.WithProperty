# Publishing to nuget.org

## Release model

`Directory.Build.props` is the single source of the package version. Ordinary PRs and pushes to `master` build and test, but **never publish**. The `publish.yml` workflow runs only when a tag named `v<Version>` is pushed. It rejects a mismatched version or a commit outside `master` history, reruns the reusable CI workflow, and publishes its exact tested `.nupkg` and matching `.snupkg`.

The new workflow replaces the old Nerdbank.GitVersioning 3.1.91 integration and permanent `NUGET_AUTH_TOKEN` secret. No version is inferred from Git history height. Source Link and repository commit metadata are still produced by the SDK.

## One-time setup by the package owner

These account settings are **not configured by merging this repository**. They require access to the owning nuget.org account and GitHub repository settings.

1. Sign in to the nuget.org profile that owns `XeonApps.Extensions.Logging.WithProperty`. Its public package page currently lists **`veikedo`** as owner. Verify that this is your intended publishing profile; a GitHub login or email address is not a substitute for a NuGet profile name.
2. In GitHub **Settings > Environments**, create an environment named **`nuget`**. Configure required reviewer approval where available and restrict deployments to release tags matching **`v*`**. Protect `master` and restrict creation/update/deletion of release tags to maintainers using repository rulesets. These controls are recommended administrative settings, not changes this PR has applied.
3. Add an environment variable or repository Actions variable named **`NUGET_USER`**, with the owning NuGet profile name (for the current owner, `veikedo`). This is a non-secret account identifier, not an API key. The publishing job fails before login if it is missing.
4. In nuget.org **Manage account > Trusted Publishing**, create a GitHub policy with the following exact values:

   | Field | Value |
   | --- | --- |
   | Repository owner | `viktor-nikolaev` |
   | Repository | `XeonApps.Extensions.Logging.WithProperty` |
   | Workflow file | `publish.yml` (filename only, not `.github/workflows/publish.yml`) |
   | Environment | `nuget` |
   | Package selection | Only `XeonApps.Extensions.Logging.WithProperty` |

   Grant only the package publishing scope required for new versions of this existing package. Select the correct package owner when configuring a policy for an organization-owned package.

`NuGet/login` obtains a GitHub OIDC token and exchanges it for a short-lived NuGet key. Only the final environment-gated publishing job has `id-token: write`; build/test jobs do not. This workflow neither reads nor needs `NUGET_AUTH_TOKEN`. After the first successful trusted release, revoke the old key on nuget.org and remove the unused GitHub secret through the appropriate account settings.

## First release: 1.3.0

Review and merge the consolidated modernization PR, then wait for the CI checks on the resulting `master` commit to pass. The older PRs #10 and #11 are superseded by the consolidated implementation; they can be closed after checking the merged changes. Do not merge their outdated workflows over the new one.

From a clean checkout of the reviewed `master` commit:

```shell
git switch master
git pull --ff-only
python3 eng/verify-package.py --tag v1.3.0
git tag -a v1.3.0 -m "Release 1.3.0"
git push origin v1.3.0
```

The tag must point at a commit reachable from `master`, and the project version at that commit must be exactly `1.3.0`. Tag creation is the deliberate publication trigger; do not create it merely to test CI before account setup is complete.

In **Actions > Publish to NuGet**, check that release validation, the three OS test jobs, package API validation, package inspection and package-consumer tests succeed. Approve the `nuget` environment deployment when prompted. The final job downloads the tested artifacts, checks their version and source commit again, obtains credentials, and pushes the package. NuGet CLI also pushes the adjacent matching `.snupkg`; do not add `--no-symbols`.

Check the package page and installation from nuget.org after indexing. Only then announce the release and close **issue #12**. Optionally create a GitHub Release using the existing tag and the matching changelog notes. A merged PR or a successful package build alone does not fulfill the request for a published package.

## Subsequent releases

Change `<Version>` in `Directory.Build.props` and update `CHANGELOG.md` through a reviewed PR. Use normal semantic versioning: patch for compatible fixes, minor for compatible additions, and major for breaking API or consumer compatibility changes. A prerelease version such as `1.3.1-rc.1` must use the exact tag `v1.3.1-rc.1`.

Keep package API validation enabled. When intentionally changing the baseline version or raising the `Microsoft.Extensions.Logging.Abstractions` minimum, review the consumer impact separately rather than accepting an automatic dependency update. The library's target framework and dependency minimum are compatibility commitments, not SDK version selectors.

## Failures and retries

A rejected tag/version or ancestry check requires correcting the release preparation; do not bypass the check. A failed test or package validation must be fixed before publishing.

For authentication failures, compare the NuGet policy's repository owner, repository, **workflow filename**, environment and package owner with the workflow and `NUGET_USER`. The environment name is case-sensitive. An API key fallback is intentionally not provided.

A failed final publishing job can be rerun from the same workflow run after fixing external configuration, while its tested artifact is retained (14 days). `--skip-duplicate` makes retries of an already accepted package non-destructive. Confirm both package and symbol publication in the job output. If artifacts have expired, rerun all jobs to reproduce and revalidate them; do not upload an untested local replacement.

NuGet versions are immutable. Never replace a released package with changed binaries under the same version or move a published release tag. Prepare a new patch version instead.

## References

- [NuGet Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing)
- [SDK Source Link integration](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/sourcelink)
- [Package validation](https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview)
- [Microsoft.Testing.Platform with dotnet test](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [xUnit.net Microsoft.Testing.Platform integration](https://xunit.net/docs/getting-started/v3/microsoft-testing-platform)
