# Command line arguments
The classes in this namespace provide an easy way for developers to create **command line tools** that can be invoked with rich command line arguments.

The **ArgumentParser** class is able to fill the properties of a complex configuration object with command line arguments provided by the caller.
## Example
This is an example of a command line call to a tool that uses this parser technique for handling command line arguments:
```text
SnBenchmark.exe -PROFILE:"Visitor:1+1" -SITE:"http://example.com" -WARMUP:20 -LIMIT:"Normal:4.0;Slow:8.0"
```

## Usage from code
The ArgumentParser class is capable of parsing the arguments above and filling the provided config object's properties with the values.

In the following example *MySetting* is a custom object created by the developer to hold configuration information. It should contain properties annotated with any of the attributes found in the *CommandLineArguments* namespace (e.g. *CommandLineArgumentAttribute*). 

```csharp
var settings = new MySettings();
try
{
    var result = ArgumentParser.Parse(args, settings);
    if(result.IsHelp)
    {
        // settings' properties are not filled, display the usage screen and exit
        Console.WriteLine(result.GetHelpText());
    }
    else
    {
        // execute main logic with a filled config object
        Run(settings);		
    }
}
catch(ParsingException e)
{
    Console.WriteLine(e.FormattedMessage);
    Console.WriteLine(e.Result.GetHelpText());
}
```

## Configuration class
The recommended design of this class is that you define *private annotated properties* for the parser to fill and expose the parsed information in public properties in a format required by your application (e.g. a string property that will be filled by the parser and a public property that exposes a list from that string). Of course if you work with only simple properties, you do not have to duplicate them.

```csharp
public class MySettings
{
	public string[] SiteUrls { get; private set; }
	private string _siteUrlArg;

	[CommandLineArgument(name: "Site", required: true, aliases: "S", helpText: "Comma separated url list (e.g.: 'http://mysite1,http://mysite1').")]
        private string SiteUrlArg
        {
            get { return _siteUrlArg; }
            set
            {
                _siteUrlArg = value;
                SiteUrls = ParseSiteUrls(value);
            }
        }
}
```