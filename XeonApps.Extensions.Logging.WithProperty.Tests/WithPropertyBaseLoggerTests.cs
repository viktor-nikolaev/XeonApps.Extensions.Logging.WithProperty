using System.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace XeonApps.Extensions.Logging.WithProperty.Tests;

public class WithPropertyBaseLoggerTests
{
    private static IReadOnlyList<KeyValuePair<string, object?>> Properties(ILogger logger) =>
        Assert.IsAssignableFrom<IReadOnlyList<KeyValuePair<string, object?>>>(logger);

    [Fact]
    public void Count_ShouldAccumulateAcrossNestedLoggers()
    {
        var logger = NullLogger.Instance.WithProperty("A", 1).WithProperty("B", 2).WithProperty("C", 3);
        Assert.Equal(3, Properties(logger).Count);
    }

    [Fact]
    public void Enumerator_ReturnsAllPropertiesFromNestedLoggers()
    {
        var logger = NullLogger.Instance.WithProperty("A", 1).WithProperty("B", 2).WithProperty("C", 3);
        Assert.Equal(new[] { "C", "B", "A" }, Properties(logger).Select(p => p.Key));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void Indexer_ThrowsForInvalidIndexOnSinglePropertyLogger(int index)
    {
        var properties = Properties(NullLogger.Instance.WithProperty("A", 1));
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties[index]);
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
        var properties = Properties(NullLogger.Instance.WithProperty("A", 1).WithProperty("B", 2).WithProperty("C", 3));
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties[index]);
        Assert.Equal("index", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(2)]
    [InlineData(int.MaxValue)]
    public void Indexer_ThrowsForInvalidIndexOnArrayBackedLogger(int index)
    {
        var properties = Properties(NullLogger.Instance.WithProperties(
            new KeyValuePair<string, object?>("A", 1), new KeyValuePair<string, object?>("B", 2)));
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties[index]);
        Assert.Equal("index", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    public void Indexer_ThrowsForInvalidIndexOnEmptyLogger(int index)
    {
        var properties = Properties(NullLogger.Instance.WithProperties(Array.Empty<KeyValuePair<string, object?>>()));
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => properties[index]);
        Assert.Equal("index", exception.ParamName);
    }

    [Theory]
    [InlineData(0, "B", 2)]
    [InlineData(1, "C", 3)]
    [InlineData(2, "A", 1)]
    public void Indexer_ReturnsPropertiesAtValidIndicesAcrossMixedLoggers(int index, string key, int value)
    {
        var properties = Properties(NullLogger.Instance.WithProperty("A", 1).WithProperties(
            new KeyValuePair<string, object?>("B", 2), new KeyValuePair<string, object?>("C", 3)));
        Assert.Equal(new KeyValuePair<string, object?>(key, value), properties[index]);
    }

    [Fact]
    public void NonGenericEnumeratorMatchesIndexerAndGenericEnumerator()
    {
        var properties = Properties(NullLogger.Instance.WithProperty("A", 1).WithProperties(("B", 2), ("C", 3)));
        var viaIndexer = Enumerable.Range(0, properties.Count).Select(i => properties[i]).ToArray();
        Assert.Equal(viaIndexer, properties.ToArray());
        Assert.Equal(viaIndexer, ((IEnumerable)properties).Cast<KeyValuePair<string, object?>>().ToArray());
    }
}
