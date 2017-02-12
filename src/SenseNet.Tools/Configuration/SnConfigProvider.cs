using System;
using System.Collections.Specialized;
using System.Configuration;

namespace SenseNet.Configuration
{
    /// <summary>
    /// Default config provider implementation that loads config values from regular
    /// config files through the built-in .Net ConfigurationManager API.
    /// </summary>
    internal class SnConfigProvider : IConfigProvider
    {
        /// <summary>
        /// Load a configuration value from the current .Net config file (app config or web.config).
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="key">Configuration key name.</param>
        /// <returns>Raw string representation of the config value as stored in the configuration.</returns>
        public string GetString(string sectionName, string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            string configValue = null;

            var section = !string.IsNullOrEmpty(sectionName)
                ? ConfigurationManager.GetSection(sectionName) as NameValueCollection
                : null;

            if (section != null)
                configValue = section[key];

            // backward compatibility: fallback to the appsettings section
            return configValue ?? ConfigurationManager.AppSettings[key];
        }
    }
}
