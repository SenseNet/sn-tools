using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    /// <inheritdoc cref="IRetrier"/>
    public class DefaultRetrier : IRetrier
    {
        private readonly ILogger<DefaultRetrier> _logger;
        private readonly RetrierOptions _options;

        public DefaultRetrier(IOptions<RetrierOptions> options, ILogger<DefaultRetrier> logger)
        {
            _logger = logger;
            _options = options.Value;
        }

        public Task RetryAsync(Func<Task> action, Func<int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null, 
            Action<Exception, int> onAfterLastIteration = null, 
            CancellationToken cancel = default)
        {
            return RetryAsync(_options.Count, _options.WaitMilliseconds, action, shouldRetry, shouldRetryOnError,
                onAfterLastIteration, cancel);
        }

        public Task<T> RetryAsync<T>(Func<Task<T>> action, Func<T, int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null,
            Action<T, Exception, int> onAfterLastIteration = null, 
            CancellationToken cancel = default)
        {
            return RetryAsync(_options.Count, _options.WaitMilliseconds, action, shouldRetry, shouldRetryOnError,
                onAfterLastIteration, cancel);
        }

        public Task RetryAsync(int count, int waitMilliseconds, Func<Task> action, Func<int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null, Action<Exception, int> onAfterLastIteration = null,
            CancellationToken cancel = default)
        {
            return RetryAsync<object>(count, waitMilliseconds, async () =>
                {
                    await action.Invoke().ConfigureAwait(false);

                    // return a dummy result
                    return null;
                },

                // Only define delegates if the caller provided custom ones - otherwise rely
                // on the default behavior by passing null. Note that we have to change the
                // parameters here because the delegates are different.
                shouldRetry: shouldRetry != null ? (_, i) => shouldRetry.Invoke(i) : null,
                shouldRetryOnError,
                onAfterLastIteration: onAfterLastIteration != null
                    ? (_, ex, iteration) => onAfterLastIteration.Invoke(ex, iteration)
                    : null, 
                cancel: cancel);
        }

        public Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> action, Func<T, int, bool> shouldRetry = null,
            Func<Exception, int, bool> shouldRetryOnError = null,
            Action<T, Exception, int> onAfterLastIteration = null, 
            CancellationToken cancel = default)
        {
            return Retrier.RetryAsync(count, waitMilliseconds, action,
                (result, i, ex) =>
                {
                    // check if the operation was cancelled before going any further
                    cancel.ThrowIfCancellationRequested();

                    var iteration = count - i + 1;

                    if (ex == null)
                    {
                        // No exception: break the cycle by default (should retry is FALSE). Or the caller may check
                        // if the result is acceptable (for example a null check) and continue trying.
                        if (shouldRetry == null || !shouldRetry(result, iteration))
                            return true;
                    }
                    else
                    {
                        // In case of an error continue trying by default (should retry on error is TRUE).
                        // The caller may decide that we should not try further and throw the exception
                        // immediately in case they do not recognize the error.
                        if (shouldRetryOnError != null && !shouldRetryOnError(ex, iteration))
                            throw ex;
                    }

                    // if the countdown is not finished, continue the cycle
                    if (i != 1)
                        return false;

                    // Last iteration: the caller may throw their own exception or suppress the error.
                    if (onAfterLastIteration != null)
                    {
                        onAfterLastIteration.Invoke(result, ex, iteration);
                    }
                    else
                    {
                        // by default we throw an exception
                        _logger.LogTrace($"Retry timeout occurred after {iteration} iterations. {ex?.Message}.");
                        throw new InvalidOperationException($"Retry timeout occurred after {iteration} iterations.", ex);
                    }

                    return true;
                }, cancel);
        }
    }
}
