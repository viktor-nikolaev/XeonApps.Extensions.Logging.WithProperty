using System;
using System.Collections.Generic;
using XeonApps.Extensions.Logging.WithProperty;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
  public static class WithPropertyLoggerExtensions
  {
    public static ILogger WithProperty(this ILogger logger, string name, object value)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }

      return new WithOnePropertyLogger(logger, new KeyValuePair<string, object>(name, value));
    }

    public static ILogger WithProperty(this ILogger logger, KeyValuePair<string, object> property)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }

      return new WithOnePropertyLogger(logger, property);
    }

    public static ILogger WithProperties(this ILogger logger, params KeyValuePair<string, object>[] properties)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }

      if (properties == null)
      {
        throw new ArgumentNullException(nameof(properties));
      }

      return new WithManyPropertiesLogger(logger, properties);
    }

    public static ILogger WithProperties(this ILogger logger, IReadOnlyList<KeyValuePair<string, object>> properties)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }

      if (properties == null)
      {
        throw new ArgumentNullException(nameof(properties));
      }

      return new WithManyPropertiesLogger(logger, properties);
    }

    public static ILogger WithProperties(this ILogger logger, params (string key, object value)[] properties)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }

      if (properties == null)
      {
        throw new ArgumentNullException(nameof(properties));
      }

      var array = new KeyValuePair<string, object>[properties.Length];
      for (var i = 0; i < properties.Length; i++)
      {
        var (key, value) = properties[i];
        array[i] = new KeyValuePair<string, object>(key, value);
      }

      return new WithManyPropertiesLogger(logger, array);
    }
  }
}