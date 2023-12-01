using System;

namespace SenseNet.Tools.Configuration;

/// <summary>
/// Defines an attribute for a class that can bind to a configuration section.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class OptionsClassAttribute : Attribute
{
    /// <summary>
    /// Gets the section path e.g. "mainsection:subsection"
    /// </summary>
    public string SectionName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OptionsClassAttribute"/> class.
    /// </summary>
    /// <remarks>
    /// The <paramref name="sectionName"/> parameter is for documentation purposes only,
    /// it is not used in any algorithmic configuration binder.
    /// </remarks>
    /// <param name="sectionName">Path of the section e.g. "mainsection:subsection".</param>
    public OptionsClassAttribute(string sectionName)
    {
        SectionName = sectionName;
    }
}