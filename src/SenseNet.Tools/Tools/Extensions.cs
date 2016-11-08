using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Tools
{
    /// <summary>
    /// Class for extension methods
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Enumerates a collection in parallel and calls an async method on each item. Useful for making 
        /// parallel async calls, e.g. independent web requests when the degree of parallelism needs to be
        /// limited.
        /// </summary>
        /// <typeparam name="T">Generic type of collection items.</typeparam>
        /// <param name="source">Source collection.</param>
        /// <param name="degreeOfParalellism">Number of partitions that the source collection is divided to.</param>
        /// <param name="action">An async action to call on each item.</param>
        /// <returns> A task tham completes when the action has completed on all items.</returns>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, int degreeOfParalellism, Func<T, Task> action)
        {
            return Task.WhenAll(Partitioner.Create(source).GetPartitions(degreeOfParalellism).Select(partition => Task.Run(async () =>
            {
                using (partition)
                    while (partition.MoveNext())
                        await action(partition.Current);
            })));
        }
    }
}
