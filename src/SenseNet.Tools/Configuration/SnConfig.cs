using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace SenseNet.Configuration
{
    /// <summary>
    /// Base class for handling configuration values. Derived classes should expose 
    /// static, strongly typed properties for well-known configuration values.
    /// </summary>
    public class SnConfig
    {
        /// <summary>
        /// Current configuration instance that serves the actual values internally. Created for testing purposes.
        /// </summary>
        internal static IConfigProvider Instance { get; set; } = new SnConfigProvider();

        /// <summary>
        /// Default config provider implementation that loads config values from regular
        /// config files through the built-in .Net ConfigurationManager API.
        /// </summary>
        private class SnConfigProvider : IConfigProvider
        {
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

        /// <summary>
        /// Gets a configuration value from the specified section, with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <typeparam name="T">Type of the value to load.</typeparam>
        /// <returns>The value found in the configuration converted to the target type, or the provided default value.</returns>
        protected internal static T GetValue<T>(string sectionName, string key, T defaultValue = default(T))
        {
            var configString = GetString(sectionName, key);

            return string.IsNullOrEmpty(configString) 
                ? defaultValue 
                : Convert<T>(configString);
        }

        /// <summary>
        /// Gets a string configuration value from the specified section, with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <returns>The value found in the configuration or the provided default value.</returns>
        protected internal static string GetString(string sectionName, string key, string defaultValue = null)
        {
            var configValue = Instance.GetString(sectionName, key);
            return configValue ?? defaultValue;
        }
        /// <summary>
        /// Gets an integer configuration value from the specified section, with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <returns>The value found in the configuration converted to an integer, or the provided default value.</returns>
        protected internal static int GetInt(string sectionName, string key, int defaultValue, int minValue = 0, int maxValue = int.MaxValue)
        {
            var value = GetValue(sectionName, key, defaultValue);
            return Math.Min(Math.Max(value, minValue), maxValue);
        }
        /// <summary>
        /// Gets a double configuration value from the specified section, with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <returns>The value found in the configuration converted to a double, or the provided default value.</returns>
        protected internal static double GetDouble(string sectionName, string key, double defaultValue, double minValue = 0, double maxValue = double.MaxValue)
        {
            var value = GetValue(sectionName, key, defaultValue);
            return Math.Min(Math.Max(value, minValue), maxValue);
        }
        /// <summary>
        /// Gets a list of values fromr the specified section and key, with a fallback to the appSettings section.
        /// The value found in config will be split by the ; and , characters with removing empty entries.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <returns>The list of items found in the configuration or an empty list.</returns>
        protected internal static List<T> GetList<T>(string sectionName, string key)
        {
            var configValue = GetValue(sectionName, key, string.Empty);

            return !string.IsNullOrEmpty(configValue)
                ? configValue.Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries).Select(Convert<T>).ToList()
                : new List<T>();
        }

        private static T Convert<T>(string value)
        {
            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value);

            return (T)System.Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
