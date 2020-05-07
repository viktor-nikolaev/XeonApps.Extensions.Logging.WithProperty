using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace XeonApps.Extensions.Logging.WithProperty
{
  internal class WithOnePropertyLogger : WithPropertyBaseLogger
  {
    private readonly KeyValuePair<string, object> _property;

    public WithOnePropertyLogger(ILogger logger, KeyValuePair<string, object> property)
      : base(logger)
    {
      _property = property;
    }

    protected override int PropertiesCount => 1;

    protected override IEnumerable<KeyValuePair<string, object>> GetProperties()
    {
      yield return _property;
    }

    protected override KeyValuePair<string, object> GetValueAt(int index)
    {
      return _property;
    }
  }
}