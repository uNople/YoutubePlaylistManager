using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupTool.Domain
{
    public interface IYouTubeCleanupToolDbContext
    {
        Task<List<PlaylistItemData>> GetPlaylistItems();
        Task<List<PlaylistData>> GetPlaylists();
        Task<List<VideoData>> GetVideos();
        Task<InsertStatus> UpsertPlaylist(PlaylistData data);
        Task<InsertStatus> UpsertPlaylistItem(PlaylistItemData data);
        Task<InsertStatus> UpsertVideo(VideoData data);
        void Migrate();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<bool> VideoExists(string id);
        Task<List<string>> GetVideoTitles();
        Task<List<IData>> FindAll(string regex);
        Task<PlaylistItemData> GetPlaylistItem(string playlistId, string videoId);
        void RemovePlaylistItem(PlaylistItemData playlistItem);
        Task<List<VideoData>> GetUncategorizedVideos(List<string> playlistTitles);
        void RemovePlaylist(string playlistId);
        Task<List<PlaylistItemData>> GetPlaylistItems(string playlistId);
    }
}