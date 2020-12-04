using System;
using System.Collections.Generic;

namespace YoutubeCleanupTool.Utils
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
    }
}
