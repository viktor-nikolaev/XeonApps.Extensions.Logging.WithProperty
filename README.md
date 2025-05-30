# XeonApps.Extensions.Logging.WithProperty
[![Version](https://img.shields.io/nuget/v/XeonApps.Extensions.Logging.WithProperty)](https://www.nuget.org/packages/XeonApps.Extensions.Logging.WithProperty)

Lightweight extension methods that attach additional properties to log messages when using `Microsoft.Extensions.Logging`. They help enrich log output with contextual data.

## Installation

```powershell
Install-Package XeonApps.Extensions.Logging.WithProperty
```

## Usage

```csharp
using Microsoft.Extensions.Logging;

ILogger logger = loggerFactory.CreateLogger<Program>();

// Add a property inline
logger
    .WithProperty("UserId", "123")
    .LogInformation("User {User} logged in", "Jon");

// Create a logger with predefined properties
logger = logger
    .WithProperty("AppVersion", "1.0")
    .WithProperties(
        ("SessionId", Guid.NewGuid()),
        ("Country", "RU")
    );

// All properties are included in each call
logger.LogInformation("Event {Event} occurred", "UserLoggedOut");
```

## Why use it?

- Enriches logs with contextual information using concise syntax.
- Works with NLog and Serilog via the standard logging abstractions.
- Supports adding single properties or sets of properties at once.
