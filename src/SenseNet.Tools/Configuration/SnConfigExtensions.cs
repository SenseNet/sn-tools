﻿using Microsoft.Extensions.Configuration;
using SenseNet.Configuration;
using SenseNet.Tools;

// ReSharper disable once CheckNamespace
namespace SenseNet.Extensions.DependencyInjection
{
    /// <summary>
    /// SnConfig extension methods.
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public static class SnConfigExtensions
    {
        /// <summary>
        /// Sets the provided configuration as the current configuration instance
        /// used by the SnConfig infrastructure.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public static IRepositoryBuilder UseConfiguration(this IRepositoryBuilder repositoryBuilder, IConfiguration configuration)
        {
            SnConfig.Instance = configuration;

            return repositoryBuilder;
        }
    }
}
