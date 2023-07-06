using System;
using Microsoft.Extensions.DependencyInjection;
using SenseNet.Tools.Mail;

// ReSharper disable once CheckNamespace
namespace SenseNet.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding email sender to the DI container.
    /// </summary>
    public static class EmailSenderExtensions
    {
        /// <summary>
        /// Registers an email sender implementation in the service collection.
        /// </summary>
        public static IServiceCollection AddSenseNetEmailSender<T>(this IServiceCollection services,
            Action<EmailOptions> configure = null)
            where T : class, IEmailSender
        {
            if (configure != null)
                services.Configure(configure);

            return services.AddSingleton<IEmailSender, T>();
        }

        /// <summary>
        /// Registers the default email sender implementation in the service collection.
        /// </summary>
        public static IServiceCollection AddSenseNetEmailSender(this IServiceCollection services,
            Action<EmailOptions> configure = null)
        {
            return services.AddSenseNetEmailSender<DefaultEmailSender>(configure);
        }
        /// <summary>
        /// Registers the null email sender implementation in the service collection.
        /// </summary>
        public static IServiceCollection AddNullEmailSender(this IServiceCollection services)
        {
            return services.AddSenseNetEmailSender<NullEmailSender>();
        }
    }
}
