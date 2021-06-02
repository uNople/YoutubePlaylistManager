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
            if (secondString == null) return false;
            return str?.StartsWith(secondString, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }

        public static bool EndsWithCi(this string str, string secondString)
        {
            if (secondString == null) return false;
            return str?.EndsWith(secondString, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }

        public static bool EqualsCi(this string str, string secondString)
        {
            return string.Equals(str, secondString, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool ContainsCi(this string str, string secondString)
        {
            if (secondString == null) return false;
            return str?.Contains(secondString, StringComparison.InvariantCultureIgnoreCase) ?? false;
        }

        /// <summary>
        /// Can't do this on the interface, otherwise we need to cast before using this.
        /// </summary>
        public static string DisplayInfo<T>(this T data) where T : IData => $"{data.Title} (Id {data.Id})";
    }
}
