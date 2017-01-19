using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Configuration
{
    /// <summary>
    /// Describes a simple interface for providing config values. It is designed to make the config handler mechanism testable.
    /// </summary>
    internal interface IConfigProvider
    {
        string GetString(string sectionName, string key);
    }
}
