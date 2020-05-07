using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace XeonApps.Extensions.Logging.WithProperty
{
  internal class WithPropertyLogger : ILogger, IReadOnlyList<KeyValuePair<string, object>>
  {
    private readonly ILogger _logger;
    private readonly WithPropertyLogger? _next;
    private readonly IReadOnlyList<KeyValuePair<string, object>>? _properties;
    private readonly int _propertiesCount;
    private readonly KeyValuePair<string, object>? _property;

    private WithPropertyLogger(ILogger logger, IReadOnlyList<KeyValuePair<string, object>>? properties,
      KeyValuePair<string, object>? property)
    {
      _properties = properties;
      _property = property;
      _propertiesCount = properties?.Count ?? 1;

      if (logger is WithPropertyLogger withPropertyLogger)
      {
        _logger = withPropertyLogger._logger;
        _next = withPropertyLogger;
        Count = _propertiesCount + withPropertyLogger.Count;
      }
      else
      {
        _logger = logger;
        Count = _propertiesCount;
      }
    }

    public WithPropertyLogger(ILogger logger, KeyValuePair<string, object> property)
      : this(logger, null, property)
    {
    }

    public WithPropertyLogger(ILogger logger, IReadOnlyList<KeyValuePair<string, object>> properties)
      : this(logger, properties, null)
    {
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
      Func<TState, Exception, string> formatter)
    {
      if (state is IEnumerable<KeyValuePair<string, object>> logValues)
      {
        var asList = logValues as IReadOnlyList<KeyValuePair<string, object>>;
        var mergedState = new WithPropertyLogger(this, asList ?? logValues.ToList());

        _logger.Log(logLevel, eventId, mergedState, exception, Formatter);
      }
      else
      {
        _logger.Log(logLevel, eventId, state, exception, formatter);
      }

      string Formatter(WithPropertyLogger x, Exception e)
      {
        return formatter(state, exception);
      }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
      return _logger.IsEnabled(logLevel);
    }

    public IDisposable BeginScope<TState>(TState state)
    {
      return _logger.BeginScope(state);
    }

    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      // it looks like this method is not getting called if we implement IReadOnlyList
      WithPropertyLogger? next = this;
      while (next != null)
      {
        if (next._properties == null)
        {
          yield return next._property!.Value;
        }
        else
        {
          foreach (var p in next._properties)
          {
            yield return p;
          }
        }

        next = next._next;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public int Count { get; }

    public KeyValuePair<string, object> this[int index]
    {
      get
      {
        if (index < _propertiesCount)
        {
          return _properties?[index] ?? _property!.Value;
        }

        if (_next == null)
        {
          throw new ArgumentOutOfRangeException(nameof(index));
        }

        return _next[index - _propertiesCount];
      }
    }
  }
}