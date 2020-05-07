using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace XeonApps.Extensions.Logging.WithProperty
{
  internal class WithManyPropertiesLogger : WithPropertyBaseLogger
  {
    private readonly IReadOnlyList<KeyValuePair<string, object>> _properties;

    public WithManyPropertiesLogger(ILogger logger, IReadOnlyList<KeyValuePair<string, object>> properties)
      : base(logger)
    {
      _properties = properties;
    }

    protected override int PropertiesCount => _properties.Count;

    protected override IEnumerable<KeyValuePair<string, object>> GetProperties()
    {
      return _properties;
    }

    protected override KeyValuePair<string, object> GetValueAt(int index)
    {
      return _properties[index];
    }
  }
}