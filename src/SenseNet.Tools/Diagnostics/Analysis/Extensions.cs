using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenseNet.Diagnostics.Analysis
{
    public static class Extensions
    {
        public static IEnumerable<TResult> Collect<TSource, TResult>(this IEnumerable<TSource> input, Func<TSource, Tuple<string, string>> keySelector, Func<TResult, TResult> finalizer = null) where TSource : Entry where TResult : EntryCollection, new()
        {
            var collector = new GenericCollector<TSource, TResult>(keySelector, finalizer);
            collector.Initialize(input);
            return collector;
        }

    }
}
