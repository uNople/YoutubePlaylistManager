using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YouTubeCleanup.Ui
{
    public interface IDoWorkOnUi
    {
        void AddOnUi<T>(ICollection<T> collection, T item);
        void RemoveOnUi<T>(ICollection<T> collection, T item);
        void ClearOnUi<T>(ICollection<T> collection);
        void RunOnUiThread(Action action);
        void RunOnUiThreadSync(Action action);
        Task RunOnUiThreadAsync(Action action);
    }
}