using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace YoutubeCleanupWpf
{
    public static class CollectionExtensions
    {
        public static void AddOnUi<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        public static void ClearOnUi<T>(this ICollection<T> collection)
        {
            Application.Current.Dispatcher.BeginInvoke(collection.Clear);
        }
    }
}