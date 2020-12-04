using Google.Apis.YouTube.v3.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Interfaces
{
    public interface IYouTubeServiceWrapper
    {
        Task<List<PlaylistItem>> GetPlaylistItems(string playlistId);
        Task<List<Video>> GetVideos(string id);
        Task<List<Playlist>> GetPlaylists();
    }
}