using System;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeCleanup.Ui
{
    public interface IUpdateDataViewModel
    {
        Task CreateNewActiveTask(Guid runGuid, string title, CancellationTokenSource cancellationTokenSource);
        Task SetActiveTaskComplete(Guid runGuid, string title);
        Task IncrementProgress();
        Task SetNewProgressMax(int progressBarMaxValue);
        Task ResetProgress();
    }
}