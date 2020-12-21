using System.Diagnostics.CodeAnalysis;

namespace YoutubeCleanupTool.Domain
{
    public class GetAndCacheYouTubeData
    {
        private readonly IYouTubeApi _youTubeApi;

        public GetAndCacheYouTubeData([NotNull] IYouTubeApi youTubeApi)
        {
            _youTubeApi = youTubeApi;
        }
    }
}
