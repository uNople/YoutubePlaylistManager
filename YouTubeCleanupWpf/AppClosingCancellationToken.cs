using System.Threading;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf;

public class AppClosingCancellationToken : IAppClosingCancellationToken
{
    public CancellationTokenSource CancellationTokenSource { get; } = new();
    public CancellationToken CancellationToken => CancellationTokenSource.Token;
}