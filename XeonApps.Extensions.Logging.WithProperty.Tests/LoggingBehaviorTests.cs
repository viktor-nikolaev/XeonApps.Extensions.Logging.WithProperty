using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace XeonApps.Extensions.Logging.WithProperty.Tests;

public class LoggingBehaviorTests
{
    [Fact]
    public void StructuredLogPreservesMessageTemplateEventExceptionAndPropertyOrder()
    {
        var sink = new RecordingLogger();
        var error = new InvalidOperationException("original");
        var logger = sink.WithProperty("A", 1).WithProperties(("B", 2), ("C", 3));
        logger.LogWarning(new EventId(42, "example"), error, "Value={Value}", 7);
        var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(sink.State);
        Assert.Equal(new[] { "Value", "{OriginalFormat}", "B", "C", "A" }, state.Select(p => p.Key));
        Assert.Equal("Value={Value}", state[1].Value);
        Assert.Equal("Value=7", sink.Message);
        Assert.Equal(LogLevel.Warning, sink.Level);
        Assert.Equal(new EventId(42, "example"), sink.EventId);
        Assert.Same(error, sink.Exception);
        Assert.Equal(1, sink.Calls);
    }

    [Fact]
    public void FormatterHonorsTheExceptionSuppliedByTheProvider()
    {
        var sink = new RecordingLogger();
        var original = new Exception("original");
        var replacement = new Exception("replacement");
        var state = new[] { new KeyValuePair<string, object?>("Value", 7) };
        sink.WithProperty("Context", 1).Log(LogLevel.Error, default, state, original,
            (value, error) => ReferenceEquals(value, state) ? error?.Message ?? "none" : "wrong state");
        Assert.NotNull(sink.Formatter);
        Assert.Equal("replacement", sink.Formatter(replacement));
        Assert.Equal("none", sink.Formatter(null));
        Assert.Same(original, sink.Exception);
    }

    [Fact]
    public void EnumerableOnlyStateIsEnumeratedOnceAndUsesTheOriginalFormatterState()
    {
        var enumerations = 0;
        IEnumerable<KeyValuePair<string, object?>> Values()
        {
            enumerations++;
            yield return new KeyValuePair<string, object?>("Value", 7);
        }
        var state = Values();
        var sink = new RecordingLogger();
        sink.WithProperty("Context", 1).Log(LogLevel.Information, default, state, null,
            (value, _) => ReferenceEquals(state, value) ? "original state" : "wrong state");
        var merged = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(sink.State);
        Assert.Equal(new[] { "Value", "Context" }, merged.Select(p => p.Key));
        Assert.Equal(2, merged.Count);
        Assert.Equal(1, enumerations);
        Assert.Equal("original state", sink.Message);
    }

    [Fact]
    public void UnstructuredStatePassesThroughWithoutChangingItsType()
    {
        var sink = new RecordingLogger();
        var state = new object();
        sink.WithProperty("Context", 1).Log(LogLevel.Information, default, state, null,
            (value, _) => ReferenceEquals(value, state) ? "unchanged" : "changed");
        Assert.Same(state, sink.State);
        Assert.Equal("unchanged", sink.Message);
    }

    [Fact]
    public void NullStatePassesThroughToTheOriginalFormatter()
    {
        var sink = new RecordingLogger();
        sink.WithProperty("Context", 1).Log<string?>(LogLevel.Information, default, null, null,
            (value, _) => value ?? "null state");
        Assert.Null(sink.State);
        Assert.Equal("null state", sink.Message);
    }

    [Fact]
    public void NullPropertyValuesArePreserved()
    {
        var sink = new RecordingLogger();
        sink.WithProperty("A", null).WithProperties(("B", null)).LogInformation("Message");
        var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(sink.State);
        Assert.Null(Assert.Single(state, p => p.Key == "A").Value);
        Assert.Null(Assert.Single(state, p => p.Key == "B").Value);
    }

    [Fact]
    public void ScopeStateAndDisposableAreForwarded()
    {
        var sink = new RecordingLogger();
        var scope = new Scope();
        var state = new object();
        sink.ScopeResult = scope;
        var result = sink.WithProperty("A", 1).WithProperty("B", 2).BeginScope(state);
        Assert.Same(state, sink.ScopeState);
        Assert.Same(scope, result);
        result!.Dispose();
        Assert.True(scope.Disposed);
    }

    [Fact]
    public void NullScopeIsForwarded()
    {
        Assert.Null(new RecordingLogger().WithProperty("A", 1).BeginScope("scope"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsEnabledDelegatesToTheUnderlyingLogger(bool enabled)
    {
        var sink = new RecordingLogger { Enabled = enabled };
        Assert.Equal(enabled, sink.WithProperty("A", 1).IsEnabled(LogLevel.Debug));
        Assert.Equal(LogLevel.Debug, sink.EnabledLevel);
    }

    [Fact]
    public void LoggerMessageDelegateRetainsStructuredProperties()
    {
        var sink = new RecordingLogger();
        var write = LoggerMessage.Define<int>(LogLevel.Information, new EventId(7), "Value={Value}");
        write(sink.WithProperty("Context", "value"), 42, null);
        var state = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(sink.State);
        Assert.Equal("value", Assert.Single(state, p => p.Key == "Context").Value);
        Assert.Equal(42, Assert.Single(state, p => p.Key == "Value").Value);
        Assert.Equal("Value=42", sink.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void AllOverloadsRejectANullLogger(int overload)
    {
        ILogger logger = null!;
        var pair = new KeyValuePair<string, object?>("A", 1);
        var error = Assert.Throws<ArgumentNullException>(() =>
        {
            switch (overload)
            {
                case 0: logger.WithProperty("A", 1); break;
                case 1: logger.WithProperty(pair); break;
                case 2: logger.WithProperties(new[] { pair }); break;
                case 3: logger.WithProperties((IReadOnlyList<KeyValuePair<string, object?>>)new[] { pair }); break;
                case 4: logger.WithProperties(("A", 1)); break;
            }
        });
        Assert.Equal("logger", error.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void CollectionOverloadsRejectNullProperties(int overload)
    {
        var logger = NullLogger.Instance;
        var error = Assert.Throws<ArgumentNullException>(() =>
        {
            switch (overload)
            {
                case 0: logger.WithProperties((KeyValuePair<string, object?>[])null!); break;
                case 1: logger.WithProperties((IReadOnlyList<KeyValuePair<string, object?>>)null!); break;
                case 2: logger.WithProperties(((string key, object? value)[])null!); break;
            }
        });
        Assert.Equal("properties", error.ParamName);
    }

    [Fact]
    public void KeyValueAndReadOnlyListOverloadsRetainProperties()
    {
        IReadOnlyList<KeyValuePair<string, object?>> values = new[] { new KeyValuePair<string, object?>("B", 2) };
        var logger = NullLogger.Instance.WithProperty(new KeyValuePair<string, object?>("A", 1)).WithProperties(values);
        var properties = Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(logger);
        Assert.Equal(new[] { "B", "A" }, properties.Select(p => p.Key));
        Assert.Equal(2, properties[0].Value);
        Assert.Equal(1, properties[1].Value);
    }

    private sealed class Scope : IDisposable
    {
        public bool Disposed { get; private set; }
        public void Dispose() => Disposed = true;
    }

    private sealed class RecordingLogger : ILogger
    {
        public object? State { get; private set; }
        public object? ScopeState { get; private set; }
        public IDisposable? ScopeResult { get; set; }
        public Exception? Exception { get; private set; }
        public string? Message { get; private set; }
        public Func<Exception?, string>? Formatter { get; private set; }
        public LogLevel Level { get; private set; }
        public EventId EventId { get; private set; }
        public int Calls { get; private set; }
        public bool Enabled { get; set; } = true;
        public LogLevel EnabledLevel { get; private set; }
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            ScopeState = state;
            return ScopeResult;
        }
        public bool IsEnabled(LogLevel logLevel)
        {
            EnabledLevel = logLevel;
            return Enabled;
        }
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            Calls++;
            State = state;
            Level = logLevel;
            EventId = eventId;
            Exception = exception;
            Formatter = error => formatter(state, error);
            Message = formatter(state, exception);
        }
    }
}
