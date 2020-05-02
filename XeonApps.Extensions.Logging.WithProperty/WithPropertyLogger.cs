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

    public WithPropertyLogger(ILogger logger, KeyValuePair<string, object> property)
    {
      _logger = logger;
      var nextSegment = (logger as WithPropertyLogger)?._segment;
      _segment = new Segment<KeyValuePair<string, object>>(property, nextSegment);
    }

    public WithPropertyLogger(ILogger logger, IReadOnlyList<KeyValuePair<string, object>> properties)
    {
      _logger = logger;
      var nextSegment = (logger as WithPropertyLogger)?._segment;
      _segment = new Segment<KeyValuePair<string, object>>(properties, nextSegment);
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