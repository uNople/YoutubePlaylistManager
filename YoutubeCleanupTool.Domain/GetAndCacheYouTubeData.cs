using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeCleanupTool.Domain
{
    public class GetAndCacheYouTubeData : IGetAndCacheYouTubeData
    {
        private readonly IYouTubeApi _youTubeApi;
        private readonly IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;

        public GetAndCacheYouTubeData([NotNull] IYouTubeApi youTubeApi,
             [NotNull] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext)
        {
            _youTubeApi = youTubeApi;
            _youTubeCleanupToolDbContext = youTubeCleanupToolDbContext;
        }

        public async Task GetPlaylists(Action<PlaylistData, InsertStatus> callback)
        {
            await foreach (var playlist in _youTubeApi.GetPlaylists())
            {
                var result = await _youTubeCleanupToolDbContext.UpsertPlaylist(playlist);
                callback(playlist, result);
            }
            await _youTubeCleanupToolDbContext.SaveChangesAsync();
        }

        public async Task GetPlaylistItems(Action<PlaylistItemData, InsertStatus> callback)
        {
            var playlists = await _youTubeCleanupToolDbContext.GetPlaylists();
            await foreach (var playlistItem in _youTubeApi.GetPlaylistItems(playlists))
            {
                var result = await _youTubeCleanupToolDbContext.UpsertPlaylistItem(playlistItem);
                callback(playlistItem, result);
            }
            await _youTubeCleanupToolDbContext.SaveChangesAsync();
        }

        public async Task GetVideos(Action<VideoData, InsertStatus> callback, bool getAllVideos, CancellationToken cancellationToken)
        {
            var playlistItems = await _youTubeCleanupToolDbContext.GetPlaylistItems();
            var videosToGet = playlistItems.Select(x => x.VideoId).ToList();
            var videosToSkip = getAllVideos ? new List<string>() : (await _youTubeCleanupToolDbContext.GetVideos()).Select(x => x.Id);
            videosToGet = videosToGet.Except(videosToSkip).ToList();
            await foreach (var video in _youTubeApi.GetVideos(videosToGet))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    callback(new VideoData { Title = "CANCELLED", Id = "CANCELLED" }, InsertStatus.Inserted);
                    return;
                }

                if (video.IsDeletedFromYouTube)
                {
                    // We only want to insert this if we haven't already - because we want to preserve any existing data we have
                    if (!await _youTubeCleanupToolDbContext.VideoExists(video.Id))
                    {
                        await UpsertVideo(callback, video);
                    }
                }
                else
                {
                    await UpsertVideo(callback, video);
                }
            }
        }

        private async Task UpsertVideo(Action<VideoData, InsertStatus> callback, VideoData video)
        {
            var result = await _youTubeCleanupToolDbContext.UpsertVideo(video);
            callback(video, result);
            await _youTubeCleanupToolDbContext.SaveChangesAsync();
        }

        public async Task GetUnicodeVideoTitles(Action<string> callback)
        {
            var videoTitles = await _youTubeCleanupToolDbContext.GetVideoTitles();
            foreach (var videoTitle in videoTitles)
            {
                if (videoTitle.ToCharArray().Any(x => ((int)x) > 1000))
                {
                    callback(videoTitle);
                }
            }
        }
    }
}
