using System;

namespace SenseNet.Configuration
{
    /// <summary>
    /// Attribute for providing a section name for configuration classes. The class
    /// marked by this attribute must inherit from SnConfig.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SectionNameAttribute : Attribute
    {
        internal string SectionName { get; }

        /// <summary>
        /// Initializes a new instance of the SectionNameAttribute class.
        /// </summary>
        /// <param name="sectionName">Name of the custom configuration section.</param>
        public SectionNameAttribute(string sectionName)
        {
            SectionName = sectionName;
        }
    }
}
