using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    /// <inheritdoc cref="IRetrier"/>
    internal class DefaultRetrier : IRetrier
    {
        private readonly ILogger<DefaultRetrier> _logger;
        private readonly RetrierOptions _options;

        public DefaultRetrier(IOptions<RetrierOptions> options, ILogger<DefaultRetrier> logger)
        {
            _logger = logger;
            _options = options.Value;
        }
        
        public Task RetryAsync(Func<Task> action, Func<Exception, int, bool> shouldRetryOnError)
        {
            return RetryAsync(_options.Count, _options.WaitMilliseconds, action, shouldRetryOnError);
        }

        public Task RetryAsync(Func<Task> action, Func<Exception, int, bool> shouldRetryOnError,
            Action<Exception, int> handleLast)
        {
            return RetryAsync(_options.Count, _options.WaitMilliseconds, action, shouldRetryOnError, handleLast);
        }

        public Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError)
        {
            return RetryAsync(_options.Count, _options.WaitMilliseconds, action, shouldRetryOnError);
        }

        public Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast)
        {
            return RetryAsync(_options.Count, _options.WaitMilliseconds, action, shouldRetryOnError, handleLast);
        }

        public Task<T> RetryAsync<T>(Func<Task<T>> action, Func<T, int, bool> shouldRetry,
            Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast)
        {
            return RetryAsync(_options.Count, _options.WaitMilliseconds, action, shouldRetry, shouldRetryOnError,
                handleLast);
        }

        public Task RetryAsync(int count, int waitMilliseconds, Func<Task> action, Func<Exception, int, bool> shouldRetryOnError)
        {
            return RetryAsync(count, waitMilliseconds, action, shouldRetryOnError, (ex, iteration) =>
            {
                _logger.LogTrace($"Retry timeout occurred after {iteration} iterations. {ex?.Message}.");
                throw new InvalidOperationException($"Retry timeout occurred after {iteration} iterations.", ex);
            });
        }
        public Task RetryAsync(int count, int waitMilliseconds, Func<Task> action, Func<Exception, int, bool> shouldRetryOnError, Action<Exception, int> handleLast)
        {
            return RetryAsync(count, waitMilliseconds, async () =>
                {
                    await action.Invoke().ConfigureAwait(false);

                    // return a dummy result
                    return true;
                },
                shouldRetryOnError,
                (_, ex, iteration) => { handleLast.Invoke(ex, iteration); });
        }

        public Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError)
        {
            return RetryAsync(count, waitMilliseconds, action, shouldRetryOnError, (_, ex, iteration) =>
            {
                _logger.LogTrace($"Retry timeout occurred after {iteration} iterations. {ex?.Message}.");
                throw new InvalidOperationException($"Retry timeout occurred after {iteration} iterations.", ex);
            });
        }

        public Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast)
        {
            // if no error occurred, we accept all kinds of results by default, so no retry is necessary
            return RetryAsync(count, waitMilliseconds, action, (_, _) => false, shouldRetryOnError, handleLast);
        }

        public Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<T, int, bool> shouldRetry, Func<Exception, int, bool> shouldRetryOnError,
            Action<T, Exception, int> handleLast)
        {
            return Retrier.RetryAsync(count, waitMilliseconds, action,
                (result, i, ex) =>
                {
                    if (ex == null)
                    {
                        // no exception, check if the result is acceptable (for example a null check)
                        if (!shouldRetry(result, count - i + 1))
                            return true;
                    }
                    else
                    {
                        // if we do not recognize the error, throw it immediately
                        if (!shouldRetryOnError(ex, count - i + 1))
                            throw ex;
                    }

                    // if the countdown is not finished, continue the cycle
                    if (i != 1)
                        return false;

                    // last iteration (caller may throw their own exception)
                    handleLast?.Invoke(result, ex, count - i + 1);
                    return true;
                });
        }
    }
}
