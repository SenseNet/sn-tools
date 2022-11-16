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
        Task RetryAsync(Func<Task> action, Func<Exception, int, bool> shouldRetryOnError);
        Task RetryAsync(Func<Task> action, Func<Exception, int, bool> shouldRetryOnError, Action<Exception, int> handleLast);

        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError);
        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError, Action<T, Exception, int> handleLast);

        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<T, int, bool> shouldRetry, Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast);

        Task RetryAsync(int count, int waitMilliseconds, Func<Task> action, Func<Exception, int, bool> shouldRetryOnError);
        Task RetryAsync(int count, int waitMilliseconds, Func<Task> action, Func<Exception, int, bool> shouldRetryOnError, Action<Exception, int> handleLast);

        Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError);
        Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError, Action<T, Exception, int> handleLast);

        Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<T, int, bool> shouldRetry, Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast);
    }
}
