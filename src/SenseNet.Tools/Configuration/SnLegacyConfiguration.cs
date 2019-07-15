using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace SenseNet.Configuration
{
    /// <summary>
    /// Legacy configuration implementation that loads config values from legacy xml
    /// config files (e.g. web.config) through the built-in .Net ConfigurationManager API.
    /// Backward compatibility: if a key is not found under the provided section, a fallback
    /// mechanism will check the 'appSettings' section for the value.
    /// </summary>
    internal class SnLegacyConfiguration : IConfiguration
    {
        /// <summary>
        /// Gets a configuration section with the specified path.
        /// </summary>
        /// <param name="key">Section path (e.g. 'mycompany/feature1').</param>
        public IConfigurationSection GetSection(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            // replace new section separator with the legacy one
            var section = ConfigurationManager.GetSection(key.Replace(':', '/')) as NameValueCollection;

            return new SnLegacyConfigSection(key, section);
        }
        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new SnNotSupportedException("Reading subsections is not supported.");
        }
        public IChangeToken GetReloadToken()
        {
            throw new SnNotSupportedException("Config token handling is not supported.");
        }

        public string this[string key]
        {
            get => ConfigurationManager.AppSettings[key];
            set => throw new SnNotSupportedException("Setting a config value is not supported.");
        }
    }

    /// <summary>
    /// Legacy configuration section implementation that is able to handle xml config subsections
    /// in old app.config and web.config files.
    /// </summary>
    internal class SnLegacyConfigSection : IConfigurationSection
    {
        private readonly NameValueCollection _configValues;

        public SnLegacyConfigSection(string name, NameValueCollection configValues)
        {
            Key = name;
            Path = name;

            _configValues = configValues;
        }

        public IConfigurationSection GetSection(string key)
        {
            throw new SnNotSupportedException("Reading subsections is not supported.");
        }
        public IEnumerable<IConfigurationSection> GetChildren()
        {
            throw new SnNotSupportedException("Reading subsections is not supported.");
        }
        public IChangeToken GetReloadToken()
        {
            throw new SnNotSupportedException("Config token handling is not supported.");
        }

        public string this[string key]
        {
            get
            {
                // special section in old configuration files
                if (string.Compare(Key, "connectionStrings", StringComparison.InvariantCultureIgnoreCase) == 0)
                    return ConfigurationManager.ConnectionStrings[key]?.ConnectionString;

                var configValue = _configValues?[key];

                // backward compatibility: fallback to the appsettings section
                return configValue ?? ConfigurationManager.AppSettings[key];
            }
            set => throw new SnNotSupportedException("Setting a section value is not supported.");
        }

        public string Key { get; }
        public string Path { get; }
        public string Value
        {
            get => throw new SnNotSupportedException("Reading whole section as a value is not supported.");
            set => throw new SnNotSupportedException("Section value setting is not supported.");
        }
    }
}
