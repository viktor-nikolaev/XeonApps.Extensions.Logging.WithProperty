using System;
using System.Collections;
using System.Collections.Generic;

namespace XeonApps.Extensions.Logging.WithProperty
{
  internal class Segment<T> : IReadOnlyList<T> where T : struct
  {
    private readonly Segment<T>? _next;
    private readonly IReadOnlyList<T>? _properties;
    private readonly T? _property;

    private Segment(IReadOnlyList<T>? properties, T? property, Segment<T>? next)
    {
      _next = next;
      _properties = properties;
      _property = property;
      Count = CurrentSegmentCount + (next?.Count ?? 0);
    }

    public Segment(IReadOnlyList<T> properties, Segment<T>? next)
      : this(properties, null, next)
    {
    }

    public Segment(T property, Segment<T>? next) : this(null, property, next)
    {
    }

    private int CurrentSegmentCount => _properties?.Count ?? 1;

    public IEnumerator<T> GetEnumerator()
    {
      // todo create cache'able iterator
      // it looks like this method is not getting called
      Segment<T>? segment = this;
      while (segment != null)
      {
        if (segment._properties == null)
        {
          yield return segment._property!.Value;
        }
        else
        {
          foreach (var p in segment._properties)
          {
            yield return p;
          }
        }

        segment = segment._next;
      }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public int Count { get; }

    public T this[int index]
    {
      get
      {
        if (index < CurrentSegmentCount)
        {
          return _properties?[index] ?? _property!.Value;
        }

        if (_next == null)
        {
          throw new ArgumentOutOfRangeException(nameof(index));
        }

        return _next[index - CurrentSegmentCount];
      }
    }
  }
}