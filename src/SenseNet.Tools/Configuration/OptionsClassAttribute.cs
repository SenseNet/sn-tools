using System;

namespace SenseNet.Tools.Configuration;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class OptionsClassAttribute : Attribute
{
    public string SectionName { get; }

    public OptionsClassAttribute(string sectionName)
    {
        SectionName = sectionName;
    }
}