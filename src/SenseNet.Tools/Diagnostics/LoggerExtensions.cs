using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SenseNet.Diagnostics;

// ReSharper disable once CheckNamespace
namespace SenseNet.Extensions.DependencyInjection
{
#pragma warning disable CS1591
    public static class LoggerExtensions
#pragma warning restore CS1591
    {
        /// <summary>
        /// Routes all log and trace messages to the official .Net ILogger interface.
        /// </summary>
        public static IServiceProvider AddSenseNetILogger(this IServiceProvider provider)
        {
            var iLogger = provider.GetService<ILogger<SnILogger>>();
            if (iLogger != null)
                SnLog.Instance = new SnILogger(iLogger);

            var iTracer = provider.GetService<ILogger<SnILoggerTracer>>();
            if (iTracer != null)
                SnTrace.SnTracers.Add(new SnILoggerTracer(iTracer));

            return provider;
        }

        /// <summary>
        /// Register the logger and tracer wrapper classes for the ILogger interface.
        /// </summary>
        public static IServiceCollection AddSenseNetILogger(this IServiceCollection services)
        {
            services.AddSingleton<IEventLogger, SnILogger>();
            services.AddSingleton<ISnTracer, SnILoggerTracer>();

            return services;
        }
    }
}
