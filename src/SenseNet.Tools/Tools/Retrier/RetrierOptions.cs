using SenseNet.Tools.Configuration;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools;

/// <summary>
/// Configuration values for the Retrier feature.
/// All properties have default values, none of them is mandatory.
/// </summary>
[OptionsClass(sectionName: "sensenet:Retrier")]
public class RetrierOptions
{
    /// <summary>
    /// Gets or sets how many times an operation is retried if the caller
    /// did not provide a different value. Default: 10.
    /// </summary>
    public int Count { get; set; } = 10;
    /// <summary>
    /// Gets or sets how many milliseconds will the module wait between two attempts. Default: 1000.
    /// </summary>
    public int WaitMilliseconds { get; set; } = 1000;
}