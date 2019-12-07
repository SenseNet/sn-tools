using Microsoft.Extensions.Configuration;
using SenseNet.Tools;

// ReSharper disable once CheckNamespace
namespace SenseNet.Configuration
{
    /// <summary>
    /// SnConfig extension methods.
    /// </summary>
    public static class SnConfigExtensions
    {
        /// <summary>
        /// Sets the provided configuration as the current configuration instance
        /// used by the SnConfig infrastructure.
        /// </summary>
        public static IRepositoryBuilder UseConfiguration(this IRepositoryBuilder repositoryBuilder, IConfiguration configuration)
        {
            SnConfig.Instance = configuration;

            return repositoryBuilder;
        }
    }
}
