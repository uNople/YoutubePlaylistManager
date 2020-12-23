using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Domain
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
    }
}