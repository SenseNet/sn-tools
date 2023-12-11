using Microsoft.Extensions.DependencyInjection;
using SenseNet.Tools.Features;

// ReSharper disable once CheckNamespace
namespace SenseNet.Extensions.DependencyInjection;

#pragma warning disable CS1591
public static class FeaturesExtensions
#pragma warning restore CS1591
{
    /// <summary>
    /// Adds a feature to the service collection.
    /// </summary>
    public static IServiceCollection AddSenseNetFeature<TFeature>(this IServiceCollection services)
        where TFeature : class, ISnFeature
    {
        services.AddSingleton<ISnFeature, TFeature>();
        
        return services;
    }
}