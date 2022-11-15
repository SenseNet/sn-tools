using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    public interface IRetrier
    {
        Task RetryAsync(Func<Task> action, Func<Exception, bool> shouldRetry);
        Task RetryAsync(Func<Task> action, Func<Exception, bool> shouldRetry, Action<Exception> handleLast);

        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, bool> shouldRetry);
        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, bool> shouldRetry, Action<T, Exception> handleLast);

        Task<T> RetryAsync<T>(Func<Task<T>> action, Func<T, bool> acceptResult, Func<Exception, bool> shouldRetry,
            Action<T, Exception> handleLast);
    }
}
