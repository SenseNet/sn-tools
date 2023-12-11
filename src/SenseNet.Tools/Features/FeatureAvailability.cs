using System;

namespace SenseNet.Tools.Features;

/// <summary>
/// Defines feature states.
/// </summary>
public enum FeatureState
{
    /// <summary>Default state.</summary>
    Unknown,
    /// <summary>The feature is available and working.</summary>
    Active,
    /// <summary>The feature is switched off globally.</summary>
    Disabled,
    /// <summary>The feature is enabled but it is not configured correctly.</summary>
    NotConfigured,
    /// <summary>The current user does not have sufficient permissions to use this feature.</summary>
    Unavailable
}

/// <summary>
/// Provides information about the availability of a feature.
/// </summary>
public record FeatureAvailability
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FeatureAvailability"/> class.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="reason"></param>
    /// <param name="lastAvailable"></param>
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