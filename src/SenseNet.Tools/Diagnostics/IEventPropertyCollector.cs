using System;
using System.Collections.Generic;
using System.Text;

namespace SenseNet.Tools.Diagnostics
{
    /// <summary>
    /// Defines an interface for property collector that can extend
    /// the event data with domain specific information.
    /// </summary>
    public interface IEventPropertyCollector
    {
        /// <summary>
        /// Extends event properties with any domain specific information.
        /// </summary>
        /// <param name="properties">Extendable property collection.</param>
        IDictionary<string, object> Collect(IDictionary<string, object> properties);
    }
}
