using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    /// <summary>
    /// Defines methods for retrying an operation based on certain conditions.
    /// </summary>
    public interface IRetrier
    {
        /// <summary>
        /// Retries an operation. Maximum number of retries and waiting time comes from configuration.
        /// </summary>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        Task RetryAsync(Func<Task> action, Func<Exception, int, bool> shouldRetryOnError);

        /// <summary>
        /// Retries an operation. Maximum number of retries and waiting time comes from configuration.
        /// </summary>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="handleLast">Custom code that will be executed after the last unsuccessful retry.</param>
        Task RetryAsync(Func<Task> action, Func<Exception, int, bool> shouldRetryOnError,
            Action<Exception, int> handleLast);

        /// <summary>
        /// Retries an operation. Maximum number of retries and waiting time comes from configuration.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError);

        /// <summary>
        /// Retries an operation. Maximum number of retries and waiting time comes from configuration.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="handleLast">Custom code that will be executed after the last unsuccessful retry.</param>
        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast);

        /// <summary>
        /// Retries an operation. Maximum number of retries and waiting time comes from configuration.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetry">If no error occurs, decides whether to retry the operation based on the result.
        /// Parameters: the result and the number of previous retries.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="handleLast">Custom code that will be executed after the last unsuccessful retry.</param>
        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<T, int, bool> shouldRetry,
            Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast);

        /// <summary>
        /// Retries an operation.
        /// </summary>
        /// <param name="count">Maximum number of retries.</param>
        /// <param name="waitMilliseconds">Waiting time in milliseconds between retries.</param>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        Task RetryAsync(int count, int waitMilliseconds, Func<Task> action,
            Func<Exception, int, bool> shouldRetryOnError);

        /// <summary>
        /// Retries an operation.
        /// </summary>
        /// <param name="count">Maximum number of retries.</param>
        /// <param name="waitMilliseconds">Waiting time in milliseconds between retries.</param>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="handleLast">Custom code that will be executed after the last unsuccessful retry.</param>
        Task RetryAsync(int count, int waitMilliseconds, Func<Task> action,
            Func<Exception, int, bool> shouldRetryOnError, Action<Exception, int> handleLast);

        /// <summary>
        /// Retries an operation.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="count">Maximum number of retries.</param>
        /// <param name="waitMilliseconds">Waiting time in milliseconds between retries.</param>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action,
            Func<Exception, int, bool> shouldRetryOnError);

        /// <summary>
        /// Retries an operation.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="count">Maximum number of retries.</param>
        /// <param name="waitMilliseconds">Waiting time in milliseconds between retries.</param>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="handleLast">Custom code that will be executed after the last unsuccessful retry.</param>
        Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action,
            Func<Exception, int, bool> shouldRetryOnError, Action<T, Exception, int> handleLast);

        /// <summary>
        /// Retries an operation.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="count">Maximum number of retries.</param>
        /// <param name="waitMilliseconds">Waiting time in milliseconds between retries.</param>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetry">If no error occurs, decides whether to retry the operation based on the result.
        /// Parameters: the result and the number of previous retries.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="handleLast">Custom code that will be executed after the last unsuccessful retry.</param>
        Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<T, int, bool> shouldRetry,
            Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast);
    }
}
