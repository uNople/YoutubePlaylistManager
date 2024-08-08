﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupTool.Domain;

public interface IYouTubeApi
{
    IAsyncEnumerable<PlaylistItemData> GetPlaylistItems(string playlistId, Func<string, Task> playlistGotDeleted);
    IAsyncEnumerable<PlaylistData> GetPlaylists();
    IAsyncEnumerable<VideoData> GetVideos(List<string> videoIdsToGet);
    Task<PlaylistItemData> AddVideoToPlaylist(string playlistId, string videoId);
    Task RemoveVideoFromPlaylist(string playlistItemId);
}