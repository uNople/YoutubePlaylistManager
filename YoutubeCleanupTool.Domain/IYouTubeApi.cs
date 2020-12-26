using System.Collections.Generic;

namespace YouTubeCleanupTool.Domain
{
    public interface IYouTubeApi
    {
        IAsyncEnumerable<PlaylistItemData> GetPlaylistItems(List<PlaylistData> playlists);
        IAsyncEnumerable<PlaylistData> GetPlaylists();
        IAsyncEnumerable<VideoData> GetVideos(List<string> videoIdsToGet);
    }
}