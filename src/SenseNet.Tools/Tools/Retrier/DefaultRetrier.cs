using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    internal class DefaultRetrier : IRetrier
    {
        private readonly ILogger<DefaultRetrier> _logger;
        private readonly RetrierOptions _options;

        public DefaultRetrier(IOptions<RetrierOptions> options, ILogger<DefaultRetrier> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task RetryAsync(Func<Task> action, Func<Exception, bool> shouldRetry)
        {
            return RetryAsync(action, shouldRetry, (ex) =>
            {
                _logger.LogTrace($"Retry error after {_options.Count} iterations: {ex.Message}.");
                throw new InvalidOperationException($"Retry timeout occurred after {_options.Count} iterations.", ex);
            });
        }
        public Task RetryAsync(Func<Task> action, Func<Exception, bool> shouldRetry, Action<Exception> handleLast)
        {
            return RetryAsync(async () =>
            {
                await action.Invoke().ConfigureAwait(false);

                // return a dummy result
                return true;
            },
                shouldRetry,
                (_, ex) => { handleLast.Invoke(ex); });
        }

        public Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, bool> shouldRetry)
        {
            return RetryAsync(action, shouldRetry, (_, ex) =>
            {
                _logger.LogTrace($"Retry error after {_options.Count} iterations: {ex.Message}.");
                throw new InvalidOperationException($"Retry timeout occurred after {_options.Count} iterations.", ex);
            });
        }
        public Task<T> RetryAsync<T>(Func<Task<T>> action, Func<Exception, bool> shouldRetry, Action<T, Exception> handleLast)
        {
            // we accept all kinds of results by default, even null
            return RetryAsync(action, _ => true, shouldRetry, handleLast);
        }

        public Task<T> RetryAsync<T>(Func<Task<T>> action, Func<T, bool> acceptResult, Func<Exception, bool> shouldRetry,
            Action<T, Exception> handleLast)
        {
            return Retrier.RetryAsync(_options.Count, _options.WaitMilliseconds, action,
                (result, i, ex) =>
                {
                    if (ex == null)
                    {
                        // no exception, check if the result is acceptable (for example a null check)
                        if (acceptResult(result))
                            return true;
                    }
                    else
                    {
                        // if we do not recognize the error, throw it immediately
                        if (!shouldRetry(ex))
                            throw ex;
                    }

                    // if the countdown is not finished, continue the cycle
                    if (i != 1)
                        return false;

                    // last iteration (caller may throw their own exception)
                    handleLast?.Invoke(result, ex);
                    return true;
                });
        }
    }
}
