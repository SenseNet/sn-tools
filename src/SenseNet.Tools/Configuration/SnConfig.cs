using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Configuration;

// ReSharper disable once CheckNamespace
namespace SenseNet.Configuration
{
    /// <summary>
    /// Base class for handling configuration values. Derived classes should expose 
    /// strongly typed properties for well-known configuration values.
    /// </summary>
    public abstract class SnConfig
    {
        /// <summary>
        /// Initializes an instance of the SnConfig class.
        /// </summary>
        // ReSharper disable once EmptyConstructor
        protected SnConfig()
        {
            // This technical constructor was created to prevent external code from
            // instantiating this utility class using the default public constructor.
        }

        //============================================================================== Properties

        /// <summary>
        /// Current configuration instance that serves the actual values internally.
        /// </summary>
        internal static IConfiguration Instance { get; set; } = new SnLegacyConfiguration();

        //============================================================================== GetValue

        /// <summary>
        /// Gets a configuration value from the specified section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <typeparam name="T">Type of the value to load.</typeparam>
        /// <returns>The value found in the configuration converted to the target type, or the provided default value.</returns>
        protected internal static T GetValue<T>(string sectionName, string key, T defaultValue = default(T))
        {
            var configString = GetString(sectionName, key);

            // not found in the config at all (distinguished from empty string!)
            if (configString == null)
                return defaultValue;

            return string.IsNullOrEmpty(configString) 
                ? defaultValue 
                : Convert<T>(configString);
        }

        /// <summary>
        /// Gets a string configuration value from the specified section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <returns>The value found in the configuration or the provided default value.</returns>
        protected internal static string GetString(string sectionName, string key, string defaultValue = null)
        {
            // if the section path contains a legacy separator ('/'), replace it with the new one (':')
            var configValue = string.IsNullOrEmpty(sectionName)
                ? Instance[key]
                : Instance.GetSection(sectionName.Replace('/', ':'))?[key];

            return configValue ?? defaultValue;
        }

        /// <summary>
        /// Gets an integer configuration value from the specified section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <returns>The value found in the configuration converted to an integer, or the provided default value.</returns>
        protected internal static int GetInt(string sectionName, string key, int defaultValue, int minValue = 0,
            int maxValue = int.MaxValue)
        {
            var value = GetValue(sectionName, key, defaultValue);
            return Math.Min(Math.Max(value, minValue), maxValue);
        }

        /// <summary>
        /// Gets a double configuration value from the specified section.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <returns>The value found in the configuration converted to a double, or the provided default value.</returns>
        protected internal static double GetDouble(string sectionName, string key, double defaultValue, 
            double minValue = 0, double maxValue = double.MaxValue)
        {
            var value = GetValue(sectionName, key, defaultValue);
            return Math.Min(Math.Max(value, minValue), maxValue);
        }

        private static readonly char[] SplitSeparatorChars = {';', ','};

        /// <summary>
        /// Gets a list of values from the specified section and key.
        /// The value found in config will be split by the ; and , characters with removing empty entries.
        /// This method returns the provided default value (which may be null!) if the key was not found. It 
        /// returns an empty list, if the key was found but the value is an empty string.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value, for a case when the key is not found in config.</param>
        /// <returns>The list of items found in the configuration or an empty list.</returns>
        protected internal static List<T> GetList<T>(string sectionName, string key, List<T> defaultValue = null)
        {
            var configValue = GetString(sectionName, key);
            if (configValue == null)
                return defaultValue;

            return !string.IsNullOrEmpty(configValue)
                ? configValue.Split(SplitSeparatorChars, StringSplitOptions.RemoveEmptyEntries).Select(Convert<T>).ToList()
                : new List<T>();
        }

        /// <summary>
        /// Gets a list of values from the specified section and key.
        /// The value found in config will be split by the ; and , characters with removing empty entries. 
        /// This method does not return null, ever. It returns an empty list, if the key was not found, 
        /// or a meaningful default value was not provided. If you want to distinguish a missing key
        /// from an empty string, please use the GetList method.
        /// </summary>
        /// <param name="sectionName">Section name (e.g. examplecompany/feature1)</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value, for a case when the key is not found in config.</param>
        /// <returns>The list of items found in the configuration or an empty list.</returns>
        protected internal static List<T> GetListOrEmpty<T>(string sectionName, string key, List<T> defaultValue = null)
        {
            return GetList(sectionName, key, defaultValue ?? new List<T>());
        }

        //============================================================================== Helper methods

        private static T Convert<T>(string value)
        {
            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value);

            return (T)System.Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
    }
}
