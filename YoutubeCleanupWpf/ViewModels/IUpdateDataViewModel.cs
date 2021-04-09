using System;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeCleanupWpf.ViewModels
{
    public interface IUpdateDataViewModel
    {
        Task PrependText(string message);
        Task CreateNewActiveTask(Guid runGuid, string title, CancellationTokenSource cancellationTokenSource);
        Task SetActiveTaskComplete(Guid runGuid, string title);
        Task IncrementProgress();
        Task SetNewProgressMax(int progressBarMaxValue);
    }
}