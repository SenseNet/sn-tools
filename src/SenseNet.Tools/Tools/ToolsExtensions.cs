using System;
using Microsoft.Extensions.DependencyInjection;
using SenseNet.Tools;

// ReSharper disable once CheckNamespace
namespace SenseNet.Extensions.DependencyInjection
{
#pragma warning disable CS1591
    public static class ToolsExtensions
#pragma warning restore CS1591
    {
        /// <summary>
        /// Adds the default retrier to the service collection.
        /// </summary>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configure">Configure retrier options.</param>
        public static IServiceCollection AddSenseNetRetrier(this IServiceCollection services, Action<RetrierOptions> configure = null)
        {
            return services.AddSenseNetRetrier<DefaultRetrier>(configure);
        }

        /// <summary>
        /// Adds the provided retrier to the service collection.
        /// </summary>
        /// <typeparam name="T">Retrier type.</typeparam>
        /// <param name="services">The IServiceCollection instance.</param>
        /// <param name="configure">Configure retrier options.</param>
        public static IServiceCollection AddSenseNetRetrier<T>(this IServiceCollection services,
            Action<RetrierOptions> configure = null) where T : class, IRetrier
        {
            return services
                .Configure<RetrierOptions>(options => { configure?.Invoke(options); })
                .AddSingleton<IRetrier, T>();
        }
    }
}
