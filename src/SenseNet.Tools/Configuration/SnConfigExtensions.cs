using Microsoft.Extensions.Configuration;
using SenseNet.Configuration;

namespace SenseNet.Tools.Configuration
{
    public static class SnConfigExtensions
    {
        public static IRepositoryBuilder UseConfiguration(this IRepositoryBuilder repositoryBuilder, IConfiguration configuration)
        {
            SnConfig.Instance = configuration;

            return repositoryBuilder;
        }
    }
}
