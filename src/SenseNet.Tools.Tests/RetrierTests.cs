using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SenseNet.Extensions.DependencyInjection;

// ReSharper disable IdentifierTypo

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class RetrierTests
    {
        // because we use this central counter to keep tract of callbacks, these tests CANNOT BE EXECUTED IN PARALLEL!
        public static int RetryCallCounter;
        public static int RetryTestValue;

        //====================================================================================== Simple actions

        [TestMethod]
        public void Retrier_Success_NoRetry()
        {
            RetryCallCounter = 0;

            Retrier.Retry(3, 10, typeof(InvalidOperationException), () =>
            {
                // run once without error
                Interlocked.Increment(ref RetryCallCounter);
            });

            Assert.AreEqual(1, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public void Retrier_Success_OneRetry()
        {
            RetryCallCounter = 0;

            Retrier.Retry(3, 10, typeof(InvalidOperationException), () =>
            {
                Interlocked.Increment(ref RetryCallCounter);

                // in the first call throw the expected exception
                if (RetryCallCounter == 1)
                    throw new InvalidOperationException();
            });

            Assert.AreEqual(2, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public void Retrier_Fail_OneRetry_DifferentException()
        {
            RetryCallCounter = 0;
            var exceptionThrown = false;

            try
            {
                Retrier.Retry(3, 10, typeof(InvalidOperationException), () =>
                {
                    Interlocked.Increment(ref RetryCallCounter);

                    // throw an unexpected exception
                    throw new ApplicationException();
                });
            }
            catch (ApplicationException)
            {
                exceptionThrown = true;
            }
            
            Assert.IsTrue(exceptionThrown, "Exception was not thrown.");
            Assert.AreEqual(1, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public void Retrier_Fail_TooManyRetries()
        {
            RetryCallCounter = 0;
            var exceptionThrown = false;

            try
            {
                Retrier.Retry(3, 10, typeof(InvalidOperationException), () =>
                {
                    Interlocked.Increment(ref RetryCallCounter);

                    // throw the expected exception
                    throw new InvalidOperationException();
                });
            }
            catch (InvalidOperationException)
            {
                exceptionThrown = true;
            }

            Assert.IsTrue(exceptionThrown, "Exception was not thrown.");
            Assert.AreEqual(3, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        //====================================================================================== Functions with return value

        //====================================================================================== Functions with return value and a condition

        [TestMethod]
        public void RetrierWithCondition_Success_NoRetry()
        {
            RetryCallCounter = 0;

            var result = Retrier.Retry(3, 10, () =>
            {
                Interlocked.Increment(ref RetryCallCounter);
                return 123;
            }, 
            (_, _, e) => e == null);

            Assert.AreEqual(123, result, "Wrong result: " + result);
            Assert.AreEqual(1, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public void RetrierWithCondition_Success_OneRetry()
        {
            RetryCallCounter = 0;

            var result = Retrier.Retry(3, 10, () =>
            {
                Interlocked.Increment(ref RetryCallCounter);

                // in the first call throw the expected exception
                if (RetryCallCounter == 1)
                    throw new InvalidOperationException();

                return 123;
            }, 
            (r, _, e) => e == null && r == 123);

            Assert.AreEqual(123, result, "Wrong result: " + result);
            Assert.AreEqual(2, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public void RetrierWithCondition_Success_ConditionsNotMet()
        {
            RetryCallCounter = 0;

            var result = Retrier.Retry(3, 10, () =>
            {
                Interlocked.Increment(ref RetryCallCounter);

                return 123;
            },
            (r, _, e) => e == null && r == 456);

            Assert.AreEqual(123, result, "Wrong result: " + result);
            Assert.AreEqual(3, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public void RetrierWithCondition_Fail_TooManyRetries()
        {
            RetryCallCounter = 0;

            var result = Retrier.Retry<int>(3, 10, () =>
            {
                Interlocked.Increment(ref RetryCallCounter);
                throw new InvalidOperationException();
            },
            (_, _, e) => e == null);

            Assert.AreEqual(default, result, "Wrong result: " + result);
            Assert.AreEqual(3, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public void RetrierWithCondition_Success_NoReturnValue()
        {
            RetryCallCounter = 0;
            RetryTestValue = 0;

            Retrier.Retry(3, 10, () =>
            {
                Interlocked.Increment(ref RetryCallCounter);

                // in the first call throw an exception
                if (RetryCallCounter == 1)
                    throw new InvalidOperationException();

                RetryTestValue++;
            },
            (_, e) => e == null);

            Assert.AreEqual(1, RetryTestValue, $"#1 RetryTestValue contains a wrong value: {RetryTestValue}.");
            Assert.AreEqual(2, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public async Task RetrierWithCondition_Success_NoReturnValueAsync()
        {
            RetryCallCounter = 0;
            RetryTestValue = 0;

            await Retrier.RetryAsync(3, 10, async () =>
            {
                Interlocked.Increment(ref RetryCallCounter);

                // in the first call throw an exception
                if (RetryCallCounter == 1)
                    throw new InvalidOperationException();

                RetryTestValue++;

                // simulate an async method call
                await Task.Delay(10);
            },
            (_, e) => e == null);

            Assert.AreEqual(1, RetryTestValue, $"#1 RetryTestValue contains a wrong value: {RetryTestValue}.");
            Assert.AreEqual(2, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        //====================================================================================== Retrier service

        [TestMethod]
        public async Task RetrierService_Void_DefaultConfig_Success()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // success immediately
            await retrier.RetryAsync(() =>
            {
                testCounter++;
                return Task.CompletedTask;
            });

            Assert.AreEqual(1, testCounter);
            testCounter = 0;

            // success after a few tries
            await retrier.RetryAsync(() =>
                {
                    testCounter++;

                    // do NOT throw on the last attempt
                    if (testCounter < 3)
                        throw new InvalidOperationException("Retry123");

                    return Task.CompletedTask;
                },
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));

            Assert.AreEqual(3, testCounter);
        }
        [TestMethod]
        public async Task RetrierService_Void_DefaultConfig_Error()
        {
            var retrier = GetRetrier();
            var testCounter = 0;
            var thrown = false;
            
            try
            {
                // always fail
                await retrier.RetryAsync(() =>
                    {
                        testCounter++;

                        throw new InvalidOperationException("Retry123");
                    },
                    shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));
            }
            catch (InvalidOperationException ex)
            {
                thrown = true;
                Assert.AreEqual("Retry timeout occurred after 3 iterations.", ex.Message);
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(3, testCounter);

            var handled = false;
            testCounter = 0;

            // always fail but suppress it
            await retrier.RetryAsync(() =>
                {
                    testCounter++;

                    throw new InvalidOperationException("Retry123");
                },
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"),
                onAfterLastIteration: (_, _) => { handled = true; });

            Assert.IsTrue(handled);
            Assert.AreEqual(3, testCounter);
        }

        [TestMethod]
        public async Task RetrierService_Generic_DefaultConfig_Success()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // success immediately
            var result = await retrier.RetryAsync(async () =>
                {
                    testCounter++;

                    // test async/await mode
                    await Task.Delay(20);

                    return testCounter;
                },
                shouldRetryOnError:(_, _) => false);

            Assert.AreEqual(1, result);
            testCounter = 0;

            // success after a few tries
            result = await retrier.RetryAsync(() =>
                {
                    testCounter++;

                    // do NOT throw on the last attempt
                    if (testCounter < 3)
                        throw new InvalidOperationException("Retry123");

                    return Task.FromResult(testCounter);
                },
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));

            Assert.AreEqual(3, result);
        }

        [TestMethod]
        public async Task RetrierService_Generic_DefaultConfig_Error()
        {
            var retrier = GetRetrier();
            var testCounter = 0;
            var thrown = false;

            try
            {
                // always fail
                await retrier.RetryAsync<int>(() =>
                    {
                        testCounter++;

                        throw new InvalidOperationException("Retry123");
                    },
                    shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));
            }
            catch (InvalidOperationException ex)
            {
                thrown = true;
                Assert.AreEqual("Retry timeout occurred after 3 iterations.", ex.Message);
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(3, testCounter);

            var handled = false;
            testCounter = 0;

            // always fail but suppress it
            var result = await retrier.RetryAsync<object>(() =>
                {
                    testCounter++;

                    throw new InvalidOperationException("Retry123");
                },
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"),
                onAfterLastIteration: (_, _, _) => { handled = true; });

            Assert.IsTrue(handled);
            Assert.AreEqual(3, testCounter);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RetrierService_Generic_DefaultConfig_Full_Success()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // success immediately
            var result = await retrier.RetryAsync(() =>
                {
                    testCounter++;

                    return Task.FromResult(testCounter);
                },
                (value, _) => value == 0,
                (_, _) => true,
                (_, _, _) => throw new InvalidOperationException("not thrown"));

            Assert.AreEqual(1, result);
            testCounter = 0;

            // success after a few tries
            result = await retrier.RetryAsync(() =>
                {
                    testCounter++;

                    // do NOT throw on the last attempt
                    if (testCounter < 3)
                        throw new InvalidOperationException("Retry123");

                    return Task.FromResult(testCounter);
                },
                (value, _) => value < 3,
                (_, _) => true,
                (_, _, _) => throw new InvalidOperationException("not thrown"));

            Assert.AreEqual(3, result);
        }
        [TestMethod]
        public async Task RetrierService_Generic_DefaultConfig_Full_Error()
        {
            var retrier = GetRetrier();
            var testCounter = 0;
            var thrown = false;

            try
            {
                // always fail
                await retrier.RetryAsync<int>(() =>
                    {
                        testCounter++;

                        throw new InvalidOperationException("Retry123");
                    },
                    (_, _) => true,
                    (ex, _) => ex.Message.Contains("Retry123"),
                    (_, ex, _) => throw new InvalidOperationException("thrown", ex));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "thrown")
                    thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(3, testCounter);

            var handled = false;
            testCounter = 0;

            // always fail but suppress it
            var result = await retrier.RetryAsync<object>(() =>
                {
                    testCounter++;

                    throw new InvalidOperationException("Retry123");
                },
                (_, _) => true,
                (ex, _) => ex.Message.Contains("Retry123"),
                (_, _, _) => { handled = true; });

            Assert.IsTrue(handled);
            Assert.AreEqual(3, testCounter);
            Assert.IsNull(result);
        }


        [TestMethod]
        public async Task RetrierService_Void_CustomConfig_Success()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // success immediately
            await retrier.RetryAsync(5, 10, () =>
                {
                    testCounter++;
                    return Task.CompletedTask;
                },
                shouldRetryOnError: (_, _) => false);

            Assert.AreEqual(1, testCounter);
            testCounter = 0;

            // success after a few tries
            await retrier.RetryAsync(5, 10, () =>
                {
                    testCounter++;

                    // do NOT throw on the last attempt
                    if (testCounter < 5)
                        throw new InvalidOperationException("Retry123");

                    return Task.CompletedTask;
                },
                shouldRetry: result => result == 0,
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));

            Assert.AreEqual(5, testCounter);
        }
        [TestMethod]
        public async Task RetrierService_Void_CustomConfig_Error()
        {
            var retrier = GetRetrier();
            var testCounter = 0;
            var thrown = false;

            try
            {
                // always fail
                await retrier.RetryAsync(5, 10, () =>
                    {
                        testCounter++;

                        throw new InvalidOperationException("Retry123");
                    },
                    shouldRetry: _ => true,
                    shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));
            }
            catch (InvalidOperationException ex)
            {
                thrown = true;
                Assert.AreEqual("Retry timeout occurred after 5 iterations.", ex.Message);
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(5, testCounter);

            var handled = false;
            testCounter = 0;

            // always fail but suppress it
            await retrier.RetryAsync(5, 10, () =>
                {
                    testCounter++;

                    throw new InvalidOperationException("Retry123");
                },
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"),
                onAfterLastIteration: (_, _) => { handled = true; });

            Assert.IsTrue(handled);
            Assert.AreEqual(5, testCounter);
        }

        [TestMethod]
        public async Task RetrierService_Generic_CustomConfig_Success()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // success immediately
            var result = await retrier.RetryAsync(5, 10, async () =>
            {
                testCounter++;

                // test async/await mode
                await Task.Delay(20);

                return testCounter;
            },
                (_, _) => false);

            Assert.AreEqual(1, result);
            testCounter = 0;

            // success after a few tries
            result = await retrier.RetryAsync(5, 10, () =>
                {
                    testCounter++;

                    // do NOT throw on the last attempt
                    if (testCounter < 5)
                        throw new InvalidOperationException("Retry123");

                    return Task.FromResult(testCounter);
                },
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));

            Assert.AreEqual(5, result);
        }

        [TestMethod]
        public async Task RetrierService_Generic_CustomConfig_Error()
        {
            var retrier = GetRetrier();
            var testCounter = 0;
            var thrown = false;

            try
            {
                // always fail
                await retrier.RetryAsync<int>(5, 10, () =>
                    {
                        testCounter++;

                        throw new InvalidOperationException("Retry123");
                    },
                    shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"));
            }
            catch (InvalidOperationException ex)
            {
                thrown = true;
                Assert.AreEqual("Retry timeout occurred after 5 iterations.", ex.Message);
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(5, testCounter);

            var handled = false;
            testCounter = 0;

            // always fail but suppress it
            var result = await retrier.RetryAsync<object>(5, 10, () =>
                {
                    testCounter++;

                    throw new InvalidOperationException("Retry123");
                },
                shouldRetryOnError: (ex, _) => ex.Message.Contains("Retry123"),
                onAfterLastIteration: (_, _, _) => { handled = true; });

            Assert.IsTrue(handled);
            Assert.AreEqual(5, testCounter);
            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task RetrierService_Generic_CustomConfig_Full_Success()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // success immediately
            var result = await retrier.RetryAsync(5, 10, () =>
                {
                    testCounter++;

                    return Task.FromResult(testCounter);
                },
                (value, _) => value == 0,
                (_, _) => true,
                (_, _, _) => throw new InvalidOperationException("not thrown"));

            Assert.AreEqual(1, result);
            testCounter = 0;

            // success after a few tries
            result = await retrier.RetryAsync(5, 10, () =>
                {
                    testCounter++;

                    // do NOT throw on the last attempt
                    if (testCounter < 5)
                        throw new InvalidOperationException("Retry123");

                    return Task.FromResult(testCounter);
                },
                (value, _) => value < 5,
                (_, _) => true,
                (_, _, _) => throw new InvalidOperationException("not thrown"));

            Assert.AreEqual(5, result);
        }
        [TestMethod]
        public async Task RetrierService_Generic_CustomConfig_Full_Error()
        {
            var retrier = GetRetrier();
            var testCounter = 0;
            var thrown = false;

            try
            {
                // always fail
                await retrier.RetryAsync<int>(5, 10, () =>
                    {
                        testCounter++;

                        throw new InvalidOperationException("Retry123");
                    },
                    (_, _) => true,
                    (ex, _) => ex.Message.Contains("Retry123"),
                    (_, _, _) => throw new InvalidOperationException("thrown"));
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message == "thrown")
                    thrown = true;
            }

            Assert.IsTrue(thrown);
            Assert.AreEqual(5, testCounter);

            var handled = false;
            testCounter = 0;

            // always fail but suppress it
            var result = await retrier.RetryAsync<object>(5, 10, () =>
                {
                    testCounter++;

                    throw new InvalidOperationException("Retry123");
                },
                (_, _) => true,
                (ex, _) => ex.Message.Contains("Retry123"),
                (_, _, _) => { handled = true; });

            Assert.IsTrue(handled);
            Assert.AreEqual(5, testCounter);
            Assert.IsNull(result);
        }

        private static IRetrier GetRetrier(Action<RetrierOptions> configure = null)
        {
            var services = new ServiceCollection()
                .AddLogging()
                .AddSenseNetRetrier(options =>
                {
                    options.Count = 3;
                    options.WaitMilliseconds = 20;

                    configure?.Invoke(options);
                })
                .BuildServiceProvider();

            return services.GetRequiredService<IRetrier>();
        }
    }
}
