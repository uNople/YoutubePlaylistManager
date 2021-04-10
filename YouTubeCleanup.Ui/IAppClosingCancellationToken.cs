using System.Threading;

namespace YouTubeCleanupWpf
{
    public interface IAppClosingCancellationToken
    {
        CancellationTokenSource CancellationTokenSource { get; }
        CancellationToken CancellationToken { get; }
    }
}