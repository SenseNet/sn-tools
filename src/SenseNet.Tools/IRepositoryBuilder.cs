// ReSharper disable UnusedMember.Global
namespace SenseNet.Tools
{
    /// <summary>
    /// Defines methods for adding or modifying providers loaded when the repository starts.
    /// </summary>
    public interface IRepositoryBuilder
    {
        /// <summary>
        /// Gets a previously set provider by type.
        /// </summary>
        T GetProvider<T>() where T : class;
        /// <summary>
        /// Gets a previously set provider by its name.
        /// </summary>
        T GetProvider<T>(string name) where T : class;
        /// <summary>
        /// Sets a provider by its name.
        /// </summary>
        void SetProvider(string providerName, object provider);
        /// <summary>
        /// Sets a provider by its type.
        /// </summary>
        void SetProvider(object provider);
    }
}
