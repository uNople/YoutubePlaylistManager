using System.Threading;

namespace YouTubeCleanup.Ui
{
    public interface IAppClosingCancellationToken
    {
        CancellationTokenSource CancellationTokenSource { get; }
        CancellationToken CancellationToken { get; }
    }
}