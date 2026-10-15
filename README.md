# XeonApps.Extensions.Logging.WithProperty

[![NuGet](https://img.shields.io/nuget/v/XeonApps.Extensions.Logging.WithProperty)](https://www.nuget.org/packages/XeonApps.Extensions.Logging.WithProperty)
[![CI](https://github.com/viktor-nikolaev/XeonApps.Extensions.Logging.WithProperty/actions/workflows/ci.yml/badge.svg)](https://github.com/viktor-nikolaev/XeonApps.Extensions.Logging.WithProperty/actions/workflows/ci.yml)

Lightweight extension methods that attach contextual properties to structured log events through `Microsoft.Extensions.Logging`, without changing the original message template.

## Installation

```shell
dotnet add package XeonApps.Extensions.Logging.WithProperty
```

## Usage

```csharp
using Microsoft.Extensions.Logging;

ILogger logger = loggerFactory.CreateLogger<Program>();

logger
    .WithProperty("UserId", "123")
    .LogInformation("User {User} logged in", "Jon");

// Reuse a logger with predefined context.
logger = logger
    .WithProperty("AppVersion", "1.0")
    .WithProperties(
        ("SessionId", Guid.NewGuid()),
        ("Country", "ES"));

logger.LogInformation("Event {Event} occurred", "UserLoggedOut");

// Null values are supported.
logger.WithProperty("OptionalValue", null).LogInformation("Operation completed");
```

The result is an `ILogger`; existing `LogInformation`, `LogWarning`, etc. calls and `LoggerMessage.Define` delegates can use it. The original logger and its category are retained. Structured-logging providers such as NLog and Serilog can consume the added key/value properties through their Microsoft logging integrations. How properties are rendered depends on the provider and its configuration.

## Compatibility and behavior

The package keeps a single **`netstandard2.0`** assembly. Its implementation does not need framework-specific APIs, so a second identical `net8.0` assembly would not add functionality. CI exercises that same assembly with the logging abstractions from **.NET 8, .NET 9, and .NET 10**, on Linux, Windows, and macOS. .NET Standard compatibility is broader than this tested matrix; older runtimes are not claimed to have been tested by the current CI.

The `Microsoft.Extensions.Logging.Abstractions` dependency remains a **minimum of 1.0.0**, not an exact-version restriction. Applications can resolve a newer version. Keeping this minimum avoids forcing existing consumers to upgrade their logging stack just to receive fixes. The library's public API is checked against the published 1.2.7 package when packing.

Structured state implementing `IEnumerable<KeyValuePair<string, object?>>` is enriched. This includes ordinary Microsoft logging message templates. Arbitrary non-structured `ILogger.Log<TState>` state is forwarded unchanged, rather than changing its type or inventing structured properties.

Message properties come first, followed by wrapper properties from newest wrapper to oldest. Duplicate names are preserved; interpretation is up to the provider. The original message template and formatter state are retained. `IsEnabled` and `BeginScope` delegate to the underlying logger.

Array and `IReadOnlyList` overloads retain the supplied collection without copying it: **do not mutate it while the logger is in use**. The tuple overload copies its input into an owned array. The wrapper does not change provider filtering, scope behavior, or thread-safety guarantees.

## Build and test

Install a stable **.NET 10 SDK** and the .NET/ASP.NET Core runtimes for **8, 9, and 10** (installing each corresponding SDK is sufficient). `global.json` selects the .NET 10 SDK and Microsoft.Testing.Platform. The tests use xUnit.net v3 with the MTP v2 integration, not the legacy VSTest adapter. Python 3.10 or later is needed only for the package-validation scripts, not to consume the library.

```shell
dotnet restore
dotnet build --configuration Release --no-restore --warnaserror
dotnet test --configuration Release --no-build --no-restore --report-trx
dotnet pack XeonApps.Extensions.Logging.WithProperty --configuration Release --output artifacts/packages --warnaserror
python3 eng/verify-package.py artifacts/packages
```

On Windows, use `python` instead of `python3` where appropriate. TRX reports are written to the runner's test-results directory unless `--results-directory` is supplied.

CI additionally restores the freshly packed package into an isolated NuGet cache and reruns the full test suite through a `PackageReference`, ensuring that tests exercise the shipped DLL and not only a project reference. Package verification checks the framework asset, dependency floor, README, license, XML docs, repository commit and portable symbol package.

## Releases

Package versions are declared in `Directory.Build.props`. The NuGet badge links to published versions; a source change or merged PR is not itself a NuGet publication.

Publication is deliberately separate from ordinary branch builds. A version-matching `v*` tag on `master` history triggers CI again; only its verified package artifacts can proceed through the `nuget` environment and NuGet Trusted Publishing. No permanent NuGet API key is stored by this workflow.

See [the release checklist](https://github.com/viktor-nikolaev/XeonApps.Extensions.Logging.WithProperty/blob/master/docs/RELEASING.md) for the one-time account setup, release commands, and recovery steps, and [the changelog](https://github.com/viktor-nikolaev/XeonApps.Extensions.Logging.WithProperty/blob/master/CHANGELOG.md) for changes.
