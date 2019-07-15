# Configuration
The *SenseNet.Configuration* namespace contains tools for developers for loading values from .Net configuration files or any other source (e.g. json or xml files). It is built on top of the built-in .Net classes but lets you **load strongly typed values** (like *int* or *bool*) and supports custom *sections*.

You can define your custom config class and publish config properties with only a few lines of code, using the loader methods available in this API.

It is compatible with the latest `IConfiguration` interface in .Net so you can initialize it with any source (e.g. the most common JSON source, which is added by default in a new web application).

## Sections and app settings
This API assumes you store your config values in **config sections** (in name-value collections).

> If you are using the legacy config implementation that builds on the .Net `ConfigurationManager` API, there is a *fallback mechanism* that tries to load the value from the central `appSettings` section, if it does not exist in the provided section.

### JSON sample
If you are using the default `appsettings.json` file in an Asp.Net application, you can define sections the usual way:

```json
{
  "exampleapp":
  {
    "switches":
    {
      "FeatureEnabled": true
    },
    "timeouts":
    {
      "TimeoutValue": 30
    }
  }
}
```

### Legacy configuration
To use sections in a .Net configuration file, you only have to define your config sections at the beginning of your config file the following way, and you can start adding values to your sections right away.

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

> Of course you may decide to use instance properties instead, this is only an example.

### Sections
When calling a loader method, you provide the section path as the first parameter. The base class will try to load values from that section first, and if not found, it will return the provided default value.

> The built-in default legacy config implementation will look for values in the `appSettings` section too (for backward compatibility reasons) if the key was not found in the provided section.

### Default values
As you can see in the example above, it is possible to provide a default value for the loader methods available in the base class. This makes it a very simple API that lets you define all the necessary information in one line when loading a config value.

## Loader methods
The loader methods published by the base *SnConfig* class give you an easy way to **load strongly typed values** from configuration files. There is a generic method for types where an automatic conversion is possible (even for *enums*), and there are other helper methods for loading arrays and specialized ones with an API for defining boundaries (e.g. min or max for *int* and *double* values).

```csharp
public class ExampleConfig : SnConfig
{
    public static string[] NetworkTargets { get; internal set; } = GetListOrEmpty<string>("exampleapp/sampleComponent", "NetworkTargets").ToArray();
    public static int MyIntValue { get; internal set; } = GetInt("exampleapp/otherSection", "myIntValue", 60, 10);
}
```

## Set up the source
If you are developing a new Asp.Net Core application, the easiest way to set up `SnConfig` in your `Startup` class is to add it using the following extension method in `ConfigureServices`:

```csharp
// The Configuration instance is the usual config source 
// in your web application, available in the Startup class.
var repositoryBuilder = new RepositoryBuilder()
  .UseConfiguration(Configuration);
```

After this you will be able to use values from your custom configuration classes above.