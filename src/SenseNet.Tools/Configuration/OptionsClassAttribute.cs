using System;

namespace SenseNet.Tools.Configuration;

/// <summary>
/// Marker attribute for sensenet options classes.
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
    /// it is not used for binding.
    /// </remarks>
    /// <param name="sectionName">Path of the section e.g. "mainsection:subsection".</param>
    public OptionsClassAttribute(string sectionName)
    {
        SectionName = sectionName;
    }
}