using System;
using System.Collections.Generic;
using XeonApps.Extensions.Logging.WithProperty;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{
  /// <summary>Creates logger wrappers that enrich structured log events with contextual properties.</summary>
  public static class WithPropertyLoggerExtensions
  {
    /// <summary>Adds one property without changing the log message template.</summary>
    /// <param name="logger">The logger to wrap.</param>
    /// <param name="name">The property name.</param>
    /// <param name="value">The property value, which may be null.</param>
    /// <returns>A logger carrying the additional property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static ILogger WithProperty(this ILogger logger, string name, object? value)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }
      return new WithOnePropertyLogger(logger, new KeyValuePair<string, object?>(name, value));
    }

    /// <summary>Adds one key/value property without changing the log message template.</summary>
    /// <param name="logger">The logger to wrap.</param>
    /// <param name="property">The property to add.</param>
    /// <returns>A logger carrying the additional property.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> is null.</exception>
    public static ILogger WithProperty(this ILogger logger, KeyValuePair<string, object?> property)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }
      return new WithOnePropertyLogger(logger, property);
    }

    /// <summary>Adds an array of properties without copying it.</summary>
    /// <remarks>Do not mutate the array while the returned logger is in use.</remarks>
    /// <param name="logger">The logger to wrap.</param>
    /// <param name="properties">The properties to add.</param>
    /// <returns>A logger carrying the additional properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="properties"/> is null.</exception>
    public static ILogger WithProperties(this ILogger logger, params KeyValuePair<string, object?>[] properties)
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

    /// <summary>Adds a read-only list of properties without copying it.</summary>
    /// <remarks>The caller must not mutate the underlying collection while the returned logger is in use.</remarks>
    /// <param name="logger">The logger to wrap.</param>
    /// <param name="properties">The properties to add.</param>
    /// <returns>A logger carrying the additional properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="properties"/> is null.</exception>
    public static ILogger WithProperties(this ILogger logger, IReadOnlyList<KeyValuePair<string, object?>> properties)
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

    /// <summary>Adds tuple properties, copying them into an owned array.</summary>
    /// <param name="logger">The logger to wrap.</param>
    /// <param name="properties">The property names and values.</param>
    /// <returns>A logger carrying the additional properties.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="logger"/> or <paramref name="properties"/> is null.</exception>
    public static ILogger WithProperties(this ILogger logger, params (string key, object? value)[] properties)
    {
      if (logger == null)
      {
        throw new ArgumentNullException(nameof(logger));
      }
      if (properties == null)
      {
        throw new ArgumentNullException(nameof(properties));
      }
      var array = new KeyValuePair<string, object?>[properties.Length];
      for (var i = 0; i < properties.Length; i++)
      {
        var (key, value) = properties[i];
        array[i] = new KeyValuePair<string, object?>(key, value);
      }
      return new WithManyPropertiesLogger(logger, array);
    }
  }
}
