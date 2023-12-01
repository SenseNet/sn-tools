using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
#pragma warning disable CS1591
    public static class SnTraceExtensions
#pragma warning restore CS1591
    {
        /// <summary>
        /// Generates a limited length trace message from a string data.
        /// </summary>
        /// <param name="text">Source data.</param>
        /// <param name="maxLength">Optional cutoff limit. Default: 100.</param>
        /// <returns></returns>
        public static string ToTrace(this string text, int maxLength = 100)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Length < maxLength ? text : text.Substring(0, maxLength);
        }

        /// <summary>
        /// Generates a limited length trace message from a set of integers.
        /// </summary>
        /// <param name="items">Source data.</param>
        /// <param name="maxCount">Optional cutoff limit. Default: 32.</param>
        /// <returns></returns>
        public static string ToTrace(this IEnumerable<int> items, int maxCount = 32) => items == null
            ? string.Empty
            : Format(items.Take(maxCount + 1).Select(x => x.ToString()).ToArray(), maxCount);

        /// <summary>
        /// Generates a limited length trace message from a set of strings.
        /// </summary>
        /// <param name="items">Source data.</param>
        /// <param name="maxCount">Optional cutoff limit. Default: 10.</param>
        /// <returns></returns>
        public static string ToTrace(this IEnumerable<string> items, int maxCount = 10) => items == null
            ? string.Empty
            : Format(items.Take(maxCount + 1).ToArray(), maxCount);

        /// <summary>
        /// Generates a trace message from max <paramref name="maxCount"/> items of a IDictionary&lt;string, string&gt; int the following format: "key1: value1, key2: value2".
        /// if an item value length is greater than 20 it is cut off and the original length is written: "very long text very long...(78)"
        /// </summary>
        /// <param name="data">Source data.</param>
        /// <param name="maxCount">Optional cutoff limit. Default: 10.</param>
        /// <returns></returns>
        public static string ToTrace(this IDictionary<string, string> data, int maxCount = 10)
        {
            if (data == null)
                return string.Empty;

            var moreItems = data.Count > maxCount ? $", ... (total count: {data.Count})" : string.Empty;

            return string.Join(", ", data.Take(maxCount).Select(x =>
            {
                if (x.Value == null)
                    return $"{x.Key}:{{null}}";
                var lenghtString = x.Value.Length > 20 ? $"({x.Value.Length})" : string.Empty;
                return $"{x.Key}:"
                       + $"{(x.Value.Length > 20 ? x.Value.Substring(0, 20) + "..." : x.Value)}"
                       + lenghtString;
            })) + moreItems;
        }

        private static string Format(string[] set, int maxCount) =>
            $"[{string.Join(", ", set.Take(maxCount))}{(set.Length > maxCount ? ", ...]" : "]")}";
    }
}
