namespace SenseNet.Configuration
{
    /// <summary>
    /// Describes a simple interface for providing config values. It is designed to make the config handler mechanism testable.
    /// </summary>
    internal interface IConfigProvider
    {
        /// <summary>
        /// Load a configuration value from the config storage (e.g. .Net config API or file system).
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="key">Configuration key name.</param>
        /// <returns>Raw string representation of the config value as stored in the configuration.</returns>
        string GetString(string sectionName, string key);
    }
}
