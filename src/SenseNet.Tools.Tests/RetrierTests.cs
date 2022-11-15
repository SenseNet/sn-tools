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
            (r, i, e) => e == null);

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
            (r, i, e) => e == null && r == 123);

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
            (r, i, e) => e == null && r == 456);

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
            (r, i, e) => e == null);

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
            (i, e) => e == null);

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
            (i, e) => e == null);

            Assert.AreEqual(1, RetryTestValue, $"#1 RetryTestValue contains a wrong value: {RetryTestValue}.");
            Assert.AreEqual(2, RetryCallCounter, $"#1 Callback called {RetryCallCounter} times.");
        }

        [TestMethod]
        public async Task RetrierService_Simple()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // success immediately
            await retrier.RetryAsync(() =>
            {
                testCounter++;
                return Task.CompletedTask;
            }, ex => false);

            Assert.AreEqual(1, testCounter);

            // success after a few tries
            await retrier.RetryAsync(() =>
            {
                testCounter++;
                
                // do NOT throw on the last attempt
                if (testCounter < 3)
                    throw new InvalidOperationException("Retry123");

                return Task.CompletedTask;
            }, ex => ex.Message.Contains("Retry123"));

            Assert.AreEqual(3, testCounter);

            try
            {
                // always fail
                await retrier.RetryAsync(() =>
                    {
                        testCounter++;

                        throw new InvalidOperationException("Retry123");
                    }, ex => ex.Message.Contains("Retry123"));
            }
            catch (InvalidOperationException ex)
            {
                Assert.AreEqual("Retry timeout occurred after 3 iterations.", ex.Message);
            }

            Assert.AreEqual(6, testCounter);
        }

        [TestMethod]
        public async Task RetrierService_Full()
        {
            var retrier = GetRetrier();
            var testCounter = 0;

            // expecting 1 retry
            var result1 = await retrier.RetryAsync(() =>
                {
                    testCounter++;
                    return Task.FromResult(testCounter);
                },
                result => result == 2,
                ex => true,
                null);

            Assert.AreEqual(2, result1);

            //UNDONE: add more Retry tests
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
