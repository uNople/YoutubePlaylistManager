using System.Threading;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class AppClosingCancellationToken : IAppClosingCancellationToken
    {
        public CancellationTokenSource CancellationTokenSource { get; }
        public CancellationToken CancellationToken => CancellationTokenSource.Token;
        public AppClosingCancellationToken()
        {
            CancellationTokenSource = new CancellationTokenSource();
        }
    }
}