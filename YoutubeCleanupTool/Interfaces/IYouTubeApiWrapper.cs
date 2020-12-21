using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using YoutubeCleanupTool.Model;

namespace YoutubeCleanupTool.Interfaces
{
    public interface IYouTubeApiWrapper
    {
        Task<List<PlaylistItemData>> GetPlaylistItems(List<PlaylistData> playlists);
        Task<List<PlaylistData>> GetPlaylists();
        IAsyncEnumerable<VideoData> GetVideos(List<PlaylistItemData> cachedPlaylistItems);
    }
}