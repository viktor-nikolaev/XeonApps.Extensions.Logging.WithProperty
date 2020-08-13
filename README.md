# XeonApps.Extensions.Logging.WithProperty
Extensions methods for adding custom properties to structured logging output

# Installation
Install-Package XeonApps.Extensions.Logging.WithProperty

# Usage

```c#
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
