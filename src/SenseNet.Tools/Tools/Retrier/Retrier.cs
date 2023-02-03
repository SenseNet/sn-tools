using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using SenseNet.Diagnostics;

// ReSharper disable IdentifierTypo

// ReSharper disable once CheckNamespace
namespace SenseNet.Tools
{
    /// <summary>
    /// Provides methods that help retrying an operation. You can provide 
    /// the maximum number of retry attempts, the callback that should be
    /// called and the exception type that should be suppressed.
    /// </summary>
    public static class Retrier
    {
        /// <summary>
        /// Calls the callback method safely. If the given type of exception is caught,
        /// waits and calls the action again. The maximum number of attempts is determined
        /// by the count parameter.
        /// </summary>
        /// <param name="count">Maximum number of attempts before throwing the caught exception.</param>
        /// <param name="waitMilliseconds">Milliseconds to wait between two attempts.</param>
        /// <param name="caughtExceptionType">Type of exception that is suppressed and triggers the next attempt.</param>
        /// <param name="callback">Void, parameterless method that the retrier executes.</param>
        [SuppressMessage("ReSharper", "CommentTypo")]
        [Obsolete("Use the async methods in the IRetrier service instead.")]
        public static void Retry(int count, int waitMilliseconds, Type caughtExceptionType, Action callback)
        {
            var retryCount = count;
            Exception lastException = null;

            while (retryCount > 0)
            {
                try
                {
                    callback();
                    return;
                }
                catch (Exception e)
                {
                    // if the thrown exception's type is different than the provided (expected) one, throw it
                    if (!caughtExceptionType.IsInstanceOfType(e))
                        throw;

                    SnTrace.System.Write($"Retrier caught an exception (countdown: {retryCount}): {e.GetType().Name}: {e.Message}");
                    lastException = e;
                    retryCount--;
                    Thread.Sleep(waitMilliseconds);
                }
            }

            if (lastException != null)
                throw lastException;
        }

        /// <summary>
        /// Calls the callback method safely. If the given type of exception is caught,
        /// waits and calls the function again. The maximum number of attempts is determined
        /// by the count parameter.
        /// </summary>
        /// <typeparam name="T">The type of the returned object.</typeparam>
        /// <param name="count">Maximum number of attempts before throwing the caught exception.</param>
        /// <param name="waitMilliseconds">Milliseconds to wait between two attempts.</param>
        /// <param name="caughtExceptionType">Type of exception that is suppressed and triggers the next attempt.</param>
        /// <param name="callback">Parameterless method with T return type.</param>
        /// <returns>Result of the callback method.</returns>
        // ReSharper disable once UnusedMember.Global
        [Obsolete("Use the async methods in the IRetrier service instead.")]
        public static T Retry<T>(int count, int waitMilliseconds, Type caughtExceptionType, Func<T> callback)
        {
            var retryCount = count;
            Exception lastException = null;

            while (retryCount > 0)
            {
                try
                {
                    return callback();
                }
                catch (Exception e)
                {
                    // if the thrown exception's type is different than the provided (expected) one, throw it
                    if (!caughtExceptionType.IsInstanceOfType(e))
                        throw;

                    SnTrace.System.Write($"Retrier caught an exception (countdown: {retryCount}): {e.GetType().Name}: {e.Message}");
                    lastException = e;
                    retryCount--;
                    Thread.Sleep(waitMilliseconds);
                }
            }

            if (lastException != null)
                throw lastException;

            // unreachable code: the last exception above is never null
            return default;
        }

        /// <summary>
        /// Performs an operation, and based on a condition it retries it a given number of times. 
        /// The checkCondition method is always called, even if there was no exception during 
        /// the operation. If it returns true, there will be no retries and the method will exit.
        /// </summary>
        /// <param name="count">Maximum number of attempts before throwing the caught exception.</param>
        /// <param name="waitMilliseconds">Milliseconds to wait between two attempts.</param>
        /// <param name="callback">Parameterless method that performs the operation that will be retried.</param>
        /// <param name="checkCondition">Function that will decide about trying again.
        /// This method must have 2 parameters with the following types in this order: int, Exception.
        /// The first parameter is the number of the current attempt, the second is the caught exception or null. 
        /// If the decider method returns with true, the main method returns immediately. Otherwise the next 
        /// attempt will be performed.
        /// </param>
        [Obsolete("Use the async methods in the IRetrier service instead.")]
        public static void Retry(int count, int waitMilliseconds, Action callback, Func<int, Exception, bool> checkCondition)
        {
            var retryCount = count;

            while (retryCount > 0)
            {
                Exception error = null;

                try
                {
                    callback();
                }
                catch (Exception e)
                {
                    SnTrace.System.Write($"Retrier caught an exception (countdown: {retryCount}): {e.GetType().Name}: {e.Message}");
                    error = e;
                }

                // We always call this, even if there was no exception: if the result satisfies the caller's
                // conditions, finish the process.
                if (checkCondition(retryCount, error))
                    break;

                retryCount--;
                Thread.Sleep(waitMilliseconds);
            }
        }

        /// <summary>
        /// Performs an operation, and based on a condition it retries it a given number of times. 
        /// The checkCondition method is always called, even if there was no exception during 
        /// the operation. If it returns true, there will be no retries and the method will exit.
        /// </summary>
        /// <typeparam name="T">The type of the returned object.</typeparam>
        /// <param name="count">Maximum number of attempts before throwing the caught exception.</param>
        /// <param name="waitMilliseconds">Milliseconds to wait between two attempts.</param>
        /// <param name="callback">Parameterless method with T return type.</param>
        /// <param name="checkCondition">Function that will decide about trying again.
        /// This method must have 3 parameters with the following types in this order: T, int, Exception.
        /// The first parameter is the return value of the current attempt, second is the number of the
        /// current attempt and the third is the caught exception or null. If this checker method 
        /// returns with true, the main method returns with the callback's result immediately. 
        /// Otherwise the next attempt will be performed.
        /// </param>
        /// <returns>Result of the callback method.</returns>
        [Obsolete("Use the async methods in the IRetrier service instead.")]
        public static T Retry<T>(int count, int waitMilliseconds, Func<T> callback, Func<T, int, Exception, bool> checkCondition)
        {
            var retryCount = count;
            var result = default(T);

            while (retryCount > 0)
            {
                Exception error = null;

                try
                {
                    result = callback();
                }
                catch (Exception e)
                {
                    SnTrace.System.Write($"Retrier caught an exception (countdown: {retryCount}): {e.GetType().Name}: {e.Message}");
                    error = e;
                }

                // We always call this, even if there was no exception: if the result satisfies the caller's
                // conditions, finish the process.
                if (checkCondition(result, retryCount, error))
                    break;

                retryCount--;
                Thread.Sleep(waitMilliseconds);
            }

            return result;
        }

        /// <summary>
        /// Performs an async operation, and based on a condition it retries it a given number of times. 
        /// The checkCondition method is always called, even if there was no exception during the operation. 
        /// If it returns true, there will be no retries and the method will exit.
        /// </summary>
        /// <typeparam name="T">The type of the returned object.</typeparam>
        /// <param name="count">Maximum number of attempts before throwing the caught exception.</param>
        /// <param name="waitMilliseconds">Milliseconds to wait between two attempts.</param>
        /// <param name="callback">Parameterless method with T return type.</param>
        /// <param name="checkCondition">Function that will decide about trying again.
        /// This method must have 3 parameters with the following types in this order: T, int, Exception.
        /// The first parameter is the return value of the current attempt, second is the number of the 
        /// current attempt and the third is the caught exception or null. If this checker method 
        /// returns with true, the main method returns with the callback's result immediately. 
        /// Otherwise the next attempt will be performed.
        /// </param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param>
        /// <returns>Result of the callback method.</returns>
        // ReSharper disable once UnusedMember.Global
        public static async Task<T> RetryAsync<T>(int count, int waitMilliseconds, Func<Task<T>> callback, 
            Func<T, int, Exception, bool> checkCondition, CancellationToken cancel = default)
        {
            var retryCount = count;
            var result = default(T);

            while (retryCount > 0)
            {
                Exception error = null;

                // check if the operation was cancelled before going any further
                cancel.ThrowIfCancellationRequested();

                try
                {
                    result = await callback().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    SnTrace.System.Write($"Retrier caught an exception (countdown: {retryCount}): {e.GetType().Name}: {e.Message}");
                    error = e;
                }

                // We always call this, even if there was no exception: if the result satisfies the caller's
                // conditions, finish the process.
                if (checkCondition(result, retryCount, error))
                    break;

                retryCount--;

                await Task.Delay(waitMilliseconds, cancel).ConfigureAwait(false);
            }

            return result;
        }

        /// <summary>
        /// Performs an operation asynchronously, and based on a condition it retries it a given number of times. 
        /// The checkCondition method is always called, even if there was no exception during the operation. 
        /// If it returns true, there will be no retries and the method will exit.
        /// </summary>
        /// <param name="count">Maximum number of attempts before throwing the caught exception.</param>
        /// <param name="waitMilliseconds">Milliseconds to wait between two attempts.</param>
        /// <param name="callback">Parameterless method that performs the operation that will be retried.</param>
        /// <param name="checkCondition">Function that will decide about trying again.
        /// This method must have 2 parameters with the following types in this order: int, Exception.
        /// The first parameter is the number of the current attempt and the second is the caught exception or null. 
        /// If this checker method returns with true, the main method returns immediately. 
        /// Otherwise the next attempt will be performed.
        /// </param>
        /// <param name="cancel">The token to monitor for cancellation requests.</param>
        public static async Task RetryAsync(int count, int waitMilliseconds, Func<Task> callback, 
            Func<int, Exception, bool> checkCondition, CancellationToken cancel = default)
        {
            var retryCount = count;

            while (retryCount > 0)
            {
                Exception error = null;

                // check if the operation was cancelled before going any further
                cancel.ThrowIfCancellationRequested();

                try
                {
                    await callback().ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    SnTrace.System.Write($"Retrier caught an exception (countdown: {retryCount}): {e.GetType().Name}: {e.Message}");
                    error = e;
                }

                // We always call this, even if there was no exception: if the result satisfies the caller's
                // conditions, finish the process.
                if (checkCondition(retryCount, error))
                    break;

                retryCount--;
                
                await Task.Delay(waitMilliseconds, cancel).ConfigureAwait(false);
            }
        }
    }
}
