using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SenseNet.Tools.Tests
{
    [TestClass]
    public class ExtensionTests
    {
        [TestMethod]
        public void Extensions_Parallel_ForEachAsync()
        {
            var parallelCount = 10;
            var finishTimes = new DateTime[parallelCount];
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            // execute long-running async tasks on each element
            Enumerable.Range(0, parallelCount).ForEachAsync(parallelCount, async i =>
            {
                await Task.Delay(2000);
                finishTimes[i] = DateTime.UtcNow;
            }).Wait();

            stopWatch.Stop();

            // the whole loop should end in 2 seconds
            Assert.IsTrue(stopWatch.Elapsed < new TimeSpan(0, 0, 0, 3));

            // All slots should contain a real datetime value 
            // (indicating that every index was touched separately).
            for (var i = 0; i < parallelCount; i++)
            {
                Assert.IsTrue(finishTimes[i] > DateTime.UtcNow.AddMinutes(-1));
            }

            // difference between end times should be small
            double delta = 0;
            for (var i = 1; i < parallelCount; i++)
            {
                delta += Math.Abs((finishTimes[i] - finishTimes[i - 1]).TotalMilliseconds);
            }

            Assert.IsTrue(delta < 50);
        }
    }
}
