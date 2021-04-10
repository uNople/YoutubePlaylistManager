using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace YouTubeCleanupWpf
{
    public class DoWorkOnUi
    {
        // TODO: Make async
        public void AddOnUi<T>(ICollection<T> collection, T item)
        {
            if (Application.Current == null)
            {
                collection.Add(item);
                return;
            }

            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, addMethod, item);
        }

        // TODO: make async
        public void RemoveOnUi<T>(ICollection<T> collection, T item)
        {
            if (Application.Current == null)
            {
                collection.Remove(item);
                return;
            }

            Func<T, bool> addMethod = collection.Remove;
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, addMethod, item);
        }

        // TODO: make async
        public void ClearOnUi<T>(ICollection<T> collection)
        {
            if (Application.Current == null)
            {
                collection.Clear();
                return;
            }

            Application.Current.Dispatcher.Invoke(collection.Clear, DispatcherPriority.Normal);
        }

        public void RunOnUiThread(Action action)
        {
            if (Application.Current == null)
            {
                action();
                return;
            }

            Application.Current.Dispatcher.BeginInvoke(
                DispatcherPriority.Normal,
                action);
        }

        public void RunOnUiThreadSync(Action action)
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
        
        public async Task RunOnUiThreadAsync(Action action)
        {
            if (Application.Current == null)
            {
                await Task.Run(action);
                return;
            }

            await Application.Current.Dispatcher.InvokeAsync(action, DispatcherPriority.Normal);
        }
    }
}