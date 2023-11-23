using System.Threading.Tasks;

namespace SenseNet.Tools.Features;

/// <summary>
/// Defines a feature that can provide its state.
/// </summary>
public interface ISnFeature
{
    /// <summary>
    /// Gets the name of the feature.
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// Gets the display name of the feature.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets the current state of the feature.
    /// </summary>
    /// <returns>A state object that provides information about the current availability of the feature.</returns>
    public Task<FeatureAvailability> GetStateAsync();
}