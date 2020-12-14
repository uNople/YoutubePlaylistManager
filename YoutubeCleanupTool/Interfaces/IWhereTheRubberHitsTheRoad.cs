using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Interfaces
{
    public interface IWhereTheRubberHitsTheRoad
    {
        Task<List<PlaylistItem>> GetPlaylistItems(List<Playlist> playlists);
        Task<List<Playlist>> GetPlaylists();
        Task<List<Video>> GetVideos(List<PlaylistItem> cachedPlaylistItems);
    }
}