#nullable enable
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeCleanupTool.Domain
{
    public interface IHttpClientWrapper
    {
        Task<byte[]> GetByteArrayAsync(string? requestUri, CancellationToken cancellationToken);
    }
}