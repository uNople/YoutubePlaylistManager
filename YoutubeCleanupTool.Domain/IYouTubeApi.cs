using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Domain
{
    public interface IYouTubeApi
    {
        IAsyncEnumerable<PlaylistItemData> GetPlaylistItems(List<PlaylistData> playlists);
        IAsyncEnumerable<PlaylistData> GetPlaylists();
        IAsyncEnumerable<VideoData> GetVideos(List<string> videoIdsToGet);
    }
}