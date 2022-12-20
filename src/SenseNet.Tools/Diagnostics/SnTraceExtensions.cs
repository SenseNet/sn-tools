using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace SenseNet.Diagnostics
{
    public static class SnTraceExtensions
    {
        public static string ToTrace(this string text, int maxLength = 100)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            return text.Length < maxLength ? text : text.Substring(0, maxLength);
        }

        public static string ToTrace(this IEnumerable<int> items, int maxCount = 32) => items == null
            ? string.Empty
            : Format(items.Take(maxCount + 1).Select(x => x.ToString()).ToArray(), maxCount);

        public static string ToTrace(this IEnumerable<string> items, int maxCount = 10) => items == null
            ? string.Empty
            : Format(items.Take(maxCount + 1).ToArray(), maxCount);

        public static string ToTrace(this IDictionary<string, string> data)
        {
            if (data == null)
                return string.Empty;

            return string.Join(", ", data.Select(x =>
            {
                if (x.Value == null)
                    return $"{x.Key}: {{null}}";
                return $"{x.Key}: " +
                       $"{(x.Value.Length > 20 ? x.Value.Substring(0, 20) + "..." : x.Value)} " +
                       $"({x.Value.Length})";
            }));
        }

        private static string Format(string[] set, int maxCount) =>
            $"[{string.Join(", ", set.Take(maxCount))}{(set.Length > maxCount ? ", ...]" : "]")}";
    }
}
