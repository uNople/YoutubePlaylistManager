using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Domain
{
    public interface IYouTubeApi
    {
        Task<List<PlaylistItemData>> GetPlaylistItems(List<PlaylistData> playlists);
        Task<List<PlaylistData>> GetPlaylists();
        IAsyncEnumerable<VideoData> GetVideos(List<string> videoIdsToGet);
    }
}