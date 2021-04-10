using System;
using System.Threading;

namespace YouTubeCleanupWpf.ViewModels
{
    public class CancellableJob
    {
        public string Name { get; set; }
        public Guid Id { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public void Cancel() => CancellationTokenSource.Cancel();
    }
}