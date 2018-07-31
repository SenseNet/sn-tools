using System.Collections.Generic;

namespace SenseNet.Tools.Diagnostics
{
    internal class BuiltInEventPropertyCollector : IEventPropertyCollector
    {
        public IDictionary<string, object> Collect(IDictionary<string, object> properties)
        {
            return properties;
        }
    }
}
