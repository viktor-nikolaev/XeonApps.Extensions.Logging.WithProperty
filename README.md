# XeonApps.Extensions.Logging.WithProperty
[![Version](https://img.shields.io/nuget/v/XeonApps.Extensions.Logging.WithProperty)](https://www.nuget.org/packages/XeonApps.Extensions.Logging.WithProperty)

Extensions methods for adding custom properties to structured logging output when using Microsoft.Extensions.Logging;

# Installation
Install-Package XeonApps.Extensions.Logging.WithProperty

See [NuGet](https://www.nuget.org/packages/XeonApps.Extensions.Logging.WithProperty/)

# Usage

```c#
using Microsoft.Extensions.Logging.Abstractions; 

ILogger logger = loggerFactory.CreateLogger<Program>();

// inline
logger
  .WithProperty("SomeProp", "value")
  .LogInformation("User {User} logged in", "Jon");

// reassign a logger with props
logger = logger
  .WithProperty("OneProp", 22)
  .WithProperty("End", 21)
  .WithProperties(
    ("key", "value")
  )
  .WithProperties(
    new KeyValuePair<string, object>("another", "one"),
    new KeyValuePair<string, object>("some", "more"),
    new KeyValuePair<string, object>("End", "more")
  );

// will have all the added props as well as props from the template 
logger.LogInformation("Event {Event} happened", "UserLoggedOut");
```

# Supported platforms

- NLog
- Serilog
