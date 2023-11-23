using Microsoft.Extensions.DependencyInjection;
using SenseNet.Tools.Features;

// ReSharper disable once CheckNamespace
namespace SenseNet.Extensions.DependencyInjection;

public static class FeaturesExtensions
{
    /// <summary>
    /// Adds a feature to the service collection.
    /// </summary>
    public static IServiceCollection AddFeature<TFeature>(this IServiceCollection services)
        where TFeature : class, ISnFeature
    {
        services.AddSingleton<ISnFeature, TFeature>();
        
        return services;
    }
}