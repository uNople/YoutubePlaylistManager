using System;
using System.Threading;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupTool.Domain
{
    public interface IGetAndCacheYouTubeData
    {
        Task GetPlaylistItems(Func<IData, InsertStatus, CancellationToken, Task> callback, CancellationToken cancellationToken);
        Task GetPlaylists(Func<IData, InsertStatus, CancellationToken, Task> callback, CancellationToken cancellationToken);
        Task GetVideos(Func<IData, InsertStatus, CancellationToken, Task> callback, bool getAllVideos, CancellationToken cancellationToken);
        Task GetUnicodeVideoTitles(Action<string> callback);
        Task<PlaylistItemData> AddVideoToPlaylist(string playlistId, string videoId);
        Task RemoveVideoFromPlaylist(string playlistId, string videoId);
        Task GetPlaylistItemsForPlaylist(Func<IData, InsertStatus, CancellationToken, Task> callback, PlaylistData playlist, CancellationToken cancellationToken);
    }
}