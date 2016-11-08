using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

            Assert.AreEqual(1, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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

            Assert.AreEqual(2, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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
            Assert.AreEqual(1, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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
            Assert.AreEqual(3, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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
            Assert.AreEqual(1, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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
            Assert.AreEqual(2, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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
            Assert.AreEqual(3, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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

            Assert.AreEqual(default(int), result, "Wrong result: " + result);
            Assert.AreEqual(3, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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

            Assert.AreEqual(1, RetryTestValue, string.Format("#1 RetryTestValue contains a wrong value: {0}.", RetryTestValue));
            Assert.AreEqual(2, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
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

            Assert.AreEqual(1, RetryTestValue, string.Format("#1 RetryTestValue contains a wrong value: {0}.", RetryTestValue));
            Assert.AreEqual(2, RetryCallCounter, string.Format("#1 Callback called {0} times.", RetryCallCounter));
        }
    }
}
