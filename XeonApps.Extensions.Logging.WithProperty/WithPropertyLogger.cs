using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace XeonApps.Extensions.Logging.WithProperty
{
  internal class WithPropertyLogger : ILogger
  {
    private readonly ILogger _logger;
    private readonly Segment<KeyValuePair<string, object>> _segment;

    private WithPropertyLogger(ILogger logger, IReadOnlyList<KeyValuePair<string, object>>? properties,
      KeyValuePair<string, object>? property)
    {
      Segment<KeyValuePair<string, object>>? nextSegment;
      if (logger is WithPropertyLogger withPropertyLogger)
      {
        _logger = withPropertyLogger._logger;
        nextSegment = withPropertyLogger._segment;
      }
      else
      {
        _logger = logger;
        nextSegment = null;
      }

      _segment = new Segment<KeyValuePair<string, object>>(properties, property, nextSegment);
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
        var segment = new Segment<KeyValuePair<string, object>>(asList ?? logValues.ToList(), _segment);
        _logger.Log(logLevel, eventId, segment, exception, (x, y) => formatter(state, y));
      }
      else
      {
        _logger.Log(logLevel, eventId, state, exception, formatter);
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
  }
}