using System;

namespace SenseNet.Tools.Features;

/// <summary>
/// Defines feature states.
/// </summary>
public enum FeatureState
{
    Unknown,
    Active,
    Disabled,
    NotConfigured,
    Unavailable
}

/// <summary>
/// Provides information about the availability of a feature.
/// </summary>
public record FeatureAvailability
{
    public FeatureAvailability(FeatureState state, string reason = null, DateTime lastAvailable = default)
    {
        State = state;
        Reason = reason;
        LastAvailable = lastAvailable;
    }

    /// <summary>
    /// State of the feature.
    /// </summary>
    public FeatureState State { get; }
    /// <summary>
    /// Reason if the feature is not available.
    /// </summary>
    public string Reason { get; }
    /// <summary>
    /// Last time the feature was available.
    /// </summary>
    public DateTime LastAvailable { get; }
}