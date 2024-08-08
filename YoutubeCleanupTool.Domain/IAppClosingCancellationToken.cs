using System.Threading;

namespace YouTubeCleanupTool.Domain;

public interface IAppClosingCancellationToken
{
    CancellationTokenSource CancellationTokenSource { get; }
    CancellationToken CancellationToken { get; }
}