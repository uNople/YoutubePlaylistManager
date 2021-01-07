using System;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeCleanupTool.Domain
{
    public interface IGetAndCacheYouTubeData
    {
        Task GetPlaylistItems(Action<PlaylistItemData, InsertStatus> callback);
        Task GetPlaylists(Action<PlaylistData, InsertStatus> callback);
        Task GetVideos(Action<VideoData, InsertStatus> callback, bool getAllVideos, CancellationToken c);
        Task GetUnicodeVideoTitles(Action<string> callback);
        Task<PlaylistItemData> AddVideoToPlaylist(string playlistId, string videoId);
    }
}