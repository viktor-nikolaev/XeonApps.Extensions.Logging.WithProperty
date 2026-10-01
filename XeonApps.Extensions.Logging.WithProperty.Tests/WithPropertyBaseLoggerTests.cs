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

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(1)]
        [InlineData(int.MaxValue)]
        public void Indexer_ThrowsForInvalidIndexOnSinglePropertyLogger(int index)
        {
            var logger = new DummyLogger().WithProperty("A", 1);
            var props = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logger);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => props[index]);

            Assert.Equal("index", exception.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(int.MaxValue)]
        public void Indexer_ThrowsForInvalidIndexOnNestedLoggers(int index)
        {
            var logger = new DummyLogger().WithProperty("A", 1).WithProperty("B", 2).WithProperty("C", 3);
            var props = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logger);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => props[index]);

            Assert.Equal("index", exception.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(2)]
        [InlineData(int.MaxValue)]
        public void Indexer_ThrowsForInvalidIndexOnArrayBackedLogger(int index)
        {
            var logger = new DummyLogger().WithProperties(
                new KeyValuePair<string, object>("A", 1),
                new KeyValuePair<string, object>("B", 2));
            var props = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logger);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => props[index]);

            Assert.Equal("index", exception.ParamName);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Indexer_ThrowsForInvalidIndexOnEmptyLogger(int index)
        {
            var logger = new DummyLogger().WithProperties(Array.Empty<KeyValuePair<string, object>>());
            var props = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logger);

            var exception = Assert.Throws<ArgumentOutOfRangeException>(() => props[index]);

            Assert.Equal("index", exception.ParamName);
        }

        [Theory]
        [InlineData(0, "B", 2)]
        [InlineData(1, "C", 3)]
        [InlineData(2, "A", 1)]
        public void Indexer_ReturnsPropertiesAtValidIndicesAcrossMixedLoggers(int index, string key, int value)
        {
            var logger = new DummyLogger().WithProperty("A", 1).WithProperties(
                new KeyValuePair<string, object>("B", 2),
                new KeyValuePair<string, object>("C", 3));
            var props = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object>>>(logger);

            Assert.Equal(new KeyValuePair<string, object>(key, value), props[index]);
        }
    }
}
