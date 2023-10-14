using SenseNet.Tools.Configuration;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools;

[OptionsClass(sectionName: "Retrier")]
public class RetrierOptions
{
    public int Count { get; set; } = 10;
    public int WaitMilliseconds { get; set; } = 1000;
}