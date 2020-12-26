using System;
using System.Collections.Generic;

namespace YouTubeCleanupTool.Domain
{
    public static class ExtensionMethods
    {
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            foreach (var v in values)
            {
                action(v);
            }
        }

        public static bool StartsWithCi(this string str, string secondString)
        {
            return str.ToUpper().StartsWith(secondString.ToUpper());
        }

        public static bool EndsWithCi(this string str, string secondString)
        {
            return str.ToUpper().EndsWith(secondString.ToUpper());
        }
    }
}
