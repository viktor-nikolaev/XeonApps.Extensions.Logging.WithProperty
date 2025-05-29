using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using XeonApps.Extensions.Logging.WithProperty;
using Xunit;

namespace XeonApps.Extensions.Logging.WithProperty.Tests
{
    public class WithPropertyBaseLoggerTests
    {
        private sealed class DummyLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => NullDisposable.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }

            private sealed class NullDisposable : IDisposable
            {
                public static readonly NullDisposable Instance = new NullDisposable();
                public void Dispose() { }
            }
        }

        [Fact]
        public void Count_ShouldAccumulateAcrossNestedLoggers()
        {
            ILogger logger = new DummyLogger();
            logger = logger.WithProperty("A", 1).WithProperty("B", 2).WithProperty("C", 3);

            var props = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logger);
            Assert.Equal(3, props.Count);
        }

        [Fact]
        public void Enumerator_ReturnsAllPropertiesFromNestedLoggers()
        {
            ILogger logger = new DummyLogger();
            logger = logger.WithProperty("A", 1).WithProperty("B", 2).WithProperty("C", 3);

            var props = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logger);
            var keys = new List<string>();
            foreach (var kv in props)
            {
                keys.Add(kv.Key);
            }

            Assert.Equal(new[] { "C", "B", "A" }, keys);
        }
    }
}
