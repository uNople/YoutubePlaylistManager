using System;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Domain
{
    public interface IGetAndCacheYouTubeData
    {
        Task GetPlaylistItems(Action<PlaylistItemData, InsertStatus> callback);
        Task GetPlaylists(Action<PlaylistData, InsertStatus> callback);
        Task GetVideos(Action<VideoData, InsertStatus> callback, bool getAllVideos);
    }
}