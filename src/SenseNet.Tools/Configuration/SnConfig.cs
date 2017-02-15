using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SenseNet.Tools;

namespace SenseNet.Configuration
{
    /// <summary>
    /// Base class for handling configuration values. Derived classes should expose 
    /// static, strongly typed properties for well-known configuration values.
    /// </summary>
    public abstract class SnConfig
    {
        //============================================================================== Properties

        /// <summary>
        /// Current configuration instance that serves the actual values internally. Created for testing purposes.
        /// </summary>
        internal static IConfigProvider Instance { get; set; } = new SnConfigProvider();

        /// <summary>
        /// A dictionary storing available config classname/sectionname pairs 
        /// for aiding automatic sectionname discovery.
        /// </summary>
        private static Dictionary<Type, string> SectionNames { get; } = TypeResolver.GetTypesByBaseType(
            typeof (SnConfig)).ToDictionary(t => t, GetSectionNameFromAttribute);

        //============================================================================== GetValue

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

            // not found in the config at all (distinguished from empty string!)
            if (configString == null)
                return defaultValue;

            return string.IsNullOrEmpty(configString) 
                ? defaultValue 
                : Convert<T>(configString);
        }

        /// <summary>
        /// Gets a configuration value from the section determined by the first type parameter, 
        /// with a fallback to the appSettings section.
        /// </summary>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <typeparam name="Q">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</typeparam>
        /// <typeparam name="T">Type of the value to load.</typeparam>
        /// <returns>The value found in the configuration converted to the target type, or the provided default value.</returns>
        protected internal static T GetValue<Q, T>(string key, T defaultValue = default(T)) where Q: SnConfig
        {
            return GetValue(typeof (Q), key, defaultValue);
        }

        /// <summary>
        /// Gets a configuration value from the section determined by the sectiontype parameter, 
        /// with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionType">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <typeparam name="T">Type of the value to load.</typeparam>
        /// <returns>The value found in the configuration converted to the target type, or the provided default value.</returns>
        protected internal static T GetValue<T>(Type sectionType, string key, T defaultValue = default(T))
        {
            var sectionName = GetSectionName(sectionType);

            return GetValue(sectionName, key, defaultValue);
        }

        //============================================================================== GetString

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
        /// Gets a string configuration value from the section determined by the type parameter, 
        /// with a fallback to the appSettings section.
        /// </summary>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <typeparam name="Q">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</typeparam>
        /// <returns>The value found in the configuration or the provided default value.</returns>
        protected internal static string GetString<Q>(string key, string defaultValue = null)
        {
            return GetString(typeof(Q), key, defaultValue);
        }

        /// <summary>
        /// Gets a string configuration value from the section determined by the sectiontype parameter, 
        /// with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionType">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <returns>The value found in the configuration or the provided default value.</returns>
        protected internal static string GetString(Type sectionType, string key, string defaultValue = null)
        {
            var sectionName = GetSectionName(sectionType);

            return GetString(sectionName, key, defaultValue);
        }

        //============================================================================== GetInt

        /// <summary>
        /// Gets an integer configuration value from the specified section, with a fallback to the appSettings section.
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
        /// Gets an integer configuration value from the section determined by the type parameter, 
        /// with a fallback to the appSettings section.
        /// </summary>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <typeparam name="Q">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</typeparam>
        /// <returns>The value found in the configuration converted to an integer, or the provided default value.</returns>
        protected internal static int GetInt<Q>(string key, int defaultValue, int minValue = 0,
            int maxValue = int.MaxValue)
        {
            return GetInt(typeof (Q), key, defaultValue, minValue, maxValue);
        }

        /// <summary>
        /// Gets an integer configuration value from the specified section, with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionType">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <returns>The value found in the configuration converted to an integer, or the provided default value.</returns>
        protected internal static int GetInt(Type sectionType, string key, int defaultValue, int minValue = 0,
            int maxValue = int.MaxValue)
        {
            var sectionName = GetSectionName(sectionType);
            return GetInt(sectionName, key, defaultValue, minValue, maxValue);
        }

        //============================================================================== GetDouble

        /// <summary>
        /// Gets a double configuration value from the specified section, with a fallback to the appSettings section.
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

        /// <summary>
        /// Gets a double configuration value from the section determined by the type parameter,
        /// with a fallback to the appSettings section.
        /// </summary>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <typeparam name="Q">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</typeparam>
        /// <returns>The value found in the configuration converted to a double, or the provided default value.</returns>
        protected internal static double GetDouble<Q>(string key, double defaultValue,
            double minValue = 0, double maxValue = double.MaxValue)
        {
            return GetDouble(typeof (Q), key, defaultValue, minValue, maxValue);
        }

        /// <summary>
        /// Gets a double configuration value from the section determined by the sectiontype parameter, 
        /// with a fallback to the appSettings section.
        /// </summary>
        /// <param name="sectionType">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value.</param>
        /// <param name="minValue">Optional minimum value. If the configured value is smaller, this minimum value will be returned instead.</param>
        /// <param name="maxValue">Optional maximum value. If the configured value is bigger, this maximum value will be returned instead.</param>
        /// <returns>The value found in the configuration converted to a double, or the provided default value.</returns>
        protected internal static double GetDouble(Type sectionType, string key, double defaultValue,
            double minValue = 0, double maxValue = double.MaxValue)
        {
            var sectionName = GetSectionName(sectionType);
            return GetDouble(sectionName, key, defaultValue, minValue, maxValue);
        }

        //============================================================================== GetList

        private static readonly char[] SplitSeparatorChars = {';', ','};

        /// <summary>
        /// Gets a list of values from the specified section and key, with a fallback to the appSettings section.
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
        /// Gets a list of values from the specified section and key, with a fallback to the appSettings section.
        /// The value found in config will be split by the ; and , characters with removing empty entries.
        /// This method returns the provided default value (which may be null!) if the key was not found. It 
        /// returns an empty list, if the key was found but the value is an empty string.
        /// </summary>
        /// <typeparam name="Q">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</typeparam>
        /// <typeparam name="T">Type of the values to load.</typeparam>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value, for a case when the key is not found in config.</param>
        /// <returns>The list of items found in the configuration or an empty list.</returns>
        protected internal static List<T> GetList<Q, T>(string key, List<T> defaultValue = null)
        {
            return GetList(typeof (Q), key, defaultValue);
        }

        /// <summary>
        /// Gets a list of values from the specified section and key, with a fallback to the appSettings section.
        /// The value found in config will be split by the ; and , characters with removing empty entries.
        /// This method returns the provided default value (which may be null!) if the key was not found. It 
        /// returns an empty list, if the key was found but the value is an empty string.
        /// </summary>
        /// <param name="sectionType">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value, for a case when the key is not found in config.</param>
        /// <typeparam name="T">Type of the values to load.</typeparam>
        /// <returns>The list of items found in the configuration or an empty list.</returns>
        protected internal static List<T> GetList<T>(Type sectionType, string key, List<T> defaultValue = null)
        {
            var sectionName = GetSectionName(sectionType);
            return GetList(sectionName, key, defaultValue);
        }

        //============================================================================== GetListOrEmpty

        /// <summary>
        /// Gets a list of values from the specified section and key, with a fallback to the appSettings section.
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

        /// <summary>
        /// Gets a list of values from the specified section and key, with a fallback to the appSettings section.
        /// The value found in config will be split by the ; and , characters with removing empty entries. 
        /// This method does not return null, ever. It returns an empty list, if the key was not found, 
        /// or a meaningful default value was not provided. If you want to distinguish a missing key
        /// from an empty string, please use the GetList method.
        /// </summary>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value, for a case when the key is not found in config.</param>
        /// <typeparam name="Q">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</typeparam>
        /// <typeparam name="T">Type of the values to load.</typeparam>
        /// <returns>The list of items found in the configuration or an empty list.</returns>
        protected internal static List<T> GetListOrEmpty<Q, T>(string key, List<T> defaultValue = null)
        {
            return GetListOrEmpty(typeof (Q), key, defaultValue);
        }

        /// <summary>
        /// Gets a list of values from the specified section and key, with a fallback to the appSettings section.
        /// The value found in config will be split by the ; and , characters with removing empty entries. 
        /// This method does not return null, ever. It returns an empty list, if the key was not found, 
        /// or a meaningful default value was not provided. If you want to distinguish a missing key
        /// from an empty string, please use the GetList method.
        /// </summary>
        /// <param name="sectionType">Type of the custom SnConfig class that represents a section.
        /// This type must be marked with the SectionName attribute.</param>
        /// <param name="key">Configuration key name.</param>
        /// <param name="defaultValue">Optional default value, for a case when the key is not found in config.</param>
        /// <typeparam name="T">Type of the values to load.</typeparam>
        /// <returns>The list of items found in the configuration or an empty list.</returns>
        protected internal static List<T> GetListOrEmpty<T>(Type sectionType, string key, List<T> defaultValue = null)
        {
            var secitionName = GetSectionName(sectionType);
            return GetListOrEmpty(secitionName, key, defaultValue);
        }

        //============================================================================== Helper methods

        private static T Convert<T>(string value)
        {
            if (typeof(T).IsEnum)
                return (T)Enum.Parse(typeof(T), value);

            return (T)System.Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }
        private static string GetSectionNameFromAttribute(Type sectionType)
        {
            var snAttribute = sectionType.GetCustomAttributes(typeof (SectionNameAttribute), true)
                .FirstOrDefault() as SectionNameAttribute;

            return snAttribute?.SectionName ?? string.Empty;
        }
        private static string GetSectionName(Type sectionType)
        {
            if (sectionType == null)
                return null;

            string sectionName;

            // Constraints:
            // - sectionType must inherit from SnConfig
            // - must be marked with the SectionName attribute containing a non-empty section name

            if (!sectionType.IsSubclassOf(typeof(SnConfig)))
                throw new InvalidOperationException($"Section type must derive from SnConfig. {sectionType.Name} does not fullfill this requirement.");

            if (!SectionNames.TryGetValue(sectionType, out sectionName) || string.IsNullOrEmpty(sectionName))
                throw new InvalidOperationException($"Please provide a valid section name for the type {sectionType.Name} using the SectionName attribute.");

            return sectionName;
        }
    }
}
