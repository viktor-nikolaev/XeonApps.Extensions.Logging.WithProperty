using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace XeonApps.Extensions.Logging.WithProperty
{
  internal abstract class WithPropertyBaseLogger : ILogger, IReadOnlyList<KeyValuePair<string, object>>
  {
    private readonly ILogger _logger;
    private readonly WithPropertyBaseLogger? _next;

    protected WithPropertyBaseLogger(ILogger logger)
    {
      if (logger is WithPropertyBaseLogger withPropertyBaseLogger)
      {
        _next = withPropertyBaseLogger;
        _logger = withPropertyBaseLogger._logger;
      }
      else
      {
        _logger = logger;
      }
    }

    protected abstract int PropertiesCount { get; }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
      Func<TState, Exception, string> formatter)
    {
      if (state is IEnumerable<KeyValuePair<string, object>> logValues)
      {
        var asList = logValues as IReadOnlyList<KeyValuePair<string, object>>;
        var mergedState = new WithManyPropertiesLogger(this, asList ?? logValues.ToList());

        _logger.Log(logLevel, eventId, mergedState, exception, Formatter);
      }
      else
      {
        _logger.Log(logLevel, eventId, state, exception, formatter);
      }

      string Formatter(WithManyPropertiesLogger x, Exception e)
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
      var result = GetProperties();
      if (_next != null)
      {
        result = result.Concat(_next);
      }

      return result.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public int Count => PropertiesCount + (_next?.PropertiesCount ?? 0);

    public KeyValuePair<string, object> this[int index]
    {
      get
      {
        if (index < PropertiesCount)
        {
          return GetValueAt(index);
        }

        if (_next == null)
        {
          throw new ArgumentOutOfRangeException(nameof(index));
        }

        return _next[index - PropertiesCount];
      }
    }

    protected abstract IEnumerable<KeyValuePair<string, object>> GetProperties();
    protected abstract KeyValuePair<string, object> GetValueAt(int index);
  }
}