# Configuration
The *SenseNet.Configuration* namespace contains tools for developers for loading values from .Net configuration files. It is built on top of the built-in .Net classes but lets you **load strongly typed values** (like *int* or *bool*) and supports custom *sections*.

You can define your custom config class and publish config properties with only a few lines of code, using the loader methods available in this API.

## Sections and app settings
This API assumes you store your config values in **custom config sections** (in name-value collections), so it looks for values there first, but as a *fallback* it tries to load the value from the central *appSettings* section too.

To make this possible, you only have to define your config sections at the beginning of your config file the following way, and you can start adding values to your sections right away.

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <sectionGroup name="exampleapp">
      <section name="switches" type="System.Configuration.NameValueFileSectionHandler" />
      <section name="timeouts" type="System.Configuration.NameValueFileSectionHandler" />
    </sectionGroup>
  </configSections>
  <switches>
    <add key="FeatureEnabled" value="false" />
  </switches>
  <timeouts>
    <add key="TimeoutValue" value="30" />
  </timeouts>
</configuration>
```

## Custom configuration class
To publish strongly typed config values in your API you only have to inherit from the *SnConfig* base class and define your properties using *auto property initializers*:

```csharp
public class ExampleConfig : SnConfig
{
    public static bool FeatureEnabled { get; internal set; } = GetValue("exampleapp/switches", "FeatureEnabled", true);
    public static int TimeoutValue { get; internal set; } = GetValue("exampleapp/timeouts", "TimeoutValue", 60);
}
```

This way your properties will be initialized only once, when the runtime first touches your config class.

### Sections
When calling a loader method, you can provide the section path as the first parameter. The base class will try to load values from that section first, and if not found (or the provided section is null), only than will turn to the central appSettings section and look for the key there.

### Default values
As you can see in the example above, it is possible to provide a default value for the loader methods available in the base class. This makes it a very simple API that lets you define all the necessary information in one line when loading a config value.

### Generic section definition
It is also possible to provide the section info for loader metheds in a strongly typed way, by defining the section once in a class attribute:

```csharp
[SectionName("exampleapp/feature1")]
public class ExampleConfig : SnConfig
{
    public static List<int> MyList { get; internal set; } = GetList<ExampleConfig, int>("MyListValue");
    public static int TimeoutValue { get; internal set; } = GetInt<ExampleConfig>("TimeoutValue", 60, 10, 100);
}
```

## Loader methods
The loader methods published by the base *SnConfig* class give you an easy way to **load strongly typed values** from configuration files. There is a generic method for types where an automatic conversion is possible (even for *enums*), and there are other helper methods for loading arrays and specialized ones with an API for defining boundaries (e.g. for *int* or *double* values).