using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3.Data;

namespace YouTubeApiWrapper.Interfaces
{
    public interface IYouTubeServiceWrapper
    {
        Task<List<PlaylistItem>> GetPlaylistItems(string playlistId);
        Task<List<Video>> GetVideos(string id);
        Task<List<Playlist>> GetPlaylists();
        Task<PlaylistItem> AddVideoToPlaylist(string playlistId, string videoId);
        Task RemoveVideoFromPlaylist(string playlistItemId);
    }
}