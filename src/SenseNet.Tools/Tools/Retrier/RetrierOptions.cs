using SenseNet.Tools.Configuration;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools;

/// <summary>
/// Provides configuration values needed by the Retrier feature.
/// All properties have default values, no configuration is mandatory.
/// </summary>
[OptionsClass(sectionName: "sensenet:Retrier")]
public class RetrierOptions
{
    /// <summary>
    /// Gets or sets the value of the retry count. Default: 10.
    /// </summary>
    public int Count { get; set; } = 10;
    /// <summary>
    /// Gets or sets the waiting milliseconds between two attempt. Default: 1000.
    /// </summary>
    public int WaitMilliseconds { get; set; } = 1000;
}