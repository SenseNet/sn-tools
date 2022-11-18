using System;
using System.Threading;
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
        /// <param name="shouldRetry">If no error occurs, decides whether to retry the operation.
        /// Default is FALSE meaning we do NOT retry if there was no error.
        /// Parameters: the number of previous retries.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Default is TRUE meaning we always retry if there was an error.
        /// Parameters: the exception (may be null) and the number of previous retries.</param>
        /// <param name="onAfterLastIteration">Custom code that will be executed after the last unsuccessful retry.
        /// Parameters: the exception (may be null) and the number of previous retries.</param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param>
        Task RetryAsync(Func<Task> action, Func<int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null,
            Action<Exception, int> onAfterLastIteration = null, 
            CancellationToken cancel = default);

        /// <summary>
        /// Retries an operation. Maximum number of retries and waiting time comes from configuration.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetry">If no error occurs, decides whether to retry the operation based on the result.
        /// Default is FALSE meaning we do NOT retry if there was no error.
        /// Parameters: the result and the number of previous retries.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Default is TRUE meaning we always retry if there was an error.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="onAfterLastIteration">Custom code that will be executed after the last unsuccessful retry.
        /// Parameters: the result, the exception (may be null) and the number of previous retries.</param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param>
        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<T, int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null,
            Action<T, Exception, int> onAfterLastIteration = null, 
            CancellationToken cancel = default);

        /// <summary>
        /// Retries an operation.
        /// </summary>
        /// <param name="count">Maximum number of retries.</param>
        /// <param name="waitMilliseconds">Waiting time in milliseconds between retries.</param>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetry">If no error occurs, decides whether to retry the operation.
        /// Default is FALSE meaning we do NOT retry if there was no error.
        /// Parameters: the number of previous retries.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Default is TRUE meaning we always retry if there was an error.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="onAfterLastIteration">Custom code that will be executed after the last unsuccessful retry.
        /// Parameters: the exception (may be null) and the number of previous retries.</param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param>
        Task RetryAsync(int count, int waitMilliseconds, Func<Task> action, Func<int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null, Action<Exception, int> onAfterLastIteration = null, 
            CancellationToken cancel = default);

        /// <summary>
        /// Retries an operation.
        /// </summary>
        /// <typeparam name="T">Type of the return value of the operation.</typeparam>
        /// <param name="count">Maximum number of retries.</param>
        /// <param name="waitMilliseconds">Waiting time in milliseconds between retries.</param>
        /// <param name="action">The operation to retry.</param>
        /// <param name="shouldRetry">If no error occurs, decides whether to retry the operation based on the result.
        /// Default is FALSE meaning we do NOT retry if there was no error.
        /// Parameters: the result and the number of previous retries.</param>
        /// <param name="shouldRetryOnError">If an error occurs, decides whether to retry the operation based on the exception.
        /// Default is TRUE meaning we always retry if there was an error.
        /// Parameters: the exception and the number of previous retries.</param>
        /// <param name="onAfterLastIteration">Custom code that will be executed after the last unsuccessful retry.
        /// Parameters: the result, the exception (may be null) and the number of previous retries.</param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param>
        Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action,
            Func<T, int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null,
            Action<T, Exception, int> onAfterLastIteration = null, 
            CancellationToken cancel = default);
    }
}