using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace YouTubeCleanupWpf
{
    public static class WpfExtensions
    {
        public static void AddOnUi<T>(this ICollection<T> collection, T item)
        {
            if (Application.Current == null)
            {
                collection.Add(item);
                return;
            }

            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        public static void RemoveOnUi<T>(this ICollection<T> collection, T item)
        {
            if (Application.Current == null)
            {
                collection.Remove(item);
                return;
            }

            Func<T, bool> addMethod = collection.Remove;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        public static void ClearOnUi<T>(this ICollection<T> collection)
        {
            if (Application.Current == null)
            {
                collection.Clear();
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(collection.Clear);
        }

        public static void RunOnUiThread(this Action action)
        {
            if (Application.Current == null)
            {
                action();
                return;
            }

            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                action);
        }
    }
}