using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupTool.Domain
{
    public class GetAndCacheYouTubeData : IGetAndCacheYouTubeData
    {
        private readonly IYouTubeApi _youTubeApi;
        private readonly IYouTubeCleanupToolDbContextFactory _youTubeCleanupToolDbContextFactory;
        private readonly IHttpClientWrapper _httpClientWrapper;
        private readonly ILogger _logger;

        public GetAndCacheYouTubeData([NotNull] IYouTubeApi youTubeApi,
            [NotNull] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            [NotNull] IHttpClientWrapper httpClientWrapper,
            [NotNull] ILogger logger
            )
        {
            _youTubeApi = youTubeApi;
            _youTubeCleanupToolDbContextFactory = youTubeCleanupToolDbContextFactory;
            _httpClientWrapper = httpClientWrapper;
            _logger = logger;
        }

        public async Task GetPlaylists(Func<IData, InsertStatus, CancellationToken, Task> callback, CancellationToken cancellationToken)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            await foreach (var playlist in _youTubeApi.GetPlaylists().WithCancellation(cancellationToken))
            {
                var result = await context.UpsertPlaylist(playlist);
                await callback(playlist, result, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task GetPlaylistItems(Func<IData, InsertStatus, CancellationToken, Task> callback, CancellationToken cancellationToken)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            var playlists = await context.GetPlaylists();
            foreach (var playlist in playlists)
            {
                await GetPlaylistItems(context, callback, playlist, cancellationToken);
            }
            
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task GetPlaylistItemsForPlaylist(Func<IData, InsertStatus, CancellationToken, Task> callback, PlaylistData playlist, CancellationToken cancellationToken)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            await GetPlaylistItems(context, callback, playlist, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        private async Task GetPlaylistItems(IYouTubeCleanupToolDbContext context, Func<IData, InsertStatus, CancellationToken, Task> callback, PlaylistData playlist, CancellationToken cancellationToken)
        {
            var playlistItems = new List<PlaylistItemData>();
            await foreach (var playlistItem in _youTubeApi.GetPlaylistItems(playlist.Id, RemovePlaylist).WithCancellation(cancellationToken))
            {
                playlistItems.Add(playlistItem);
                var result = await context.UpsertPlaylistItem(playlistItem);
                await callback(playlistItem, result, cancellationToken);
                cancellationToken.ThrowIfCancellationRequested();
            }

            // Now we have to remove playlist items we didn't get back from the API - Otherwise if we delete + add items then we end up with duplicates
            var originalPlaylistItems = await context.GetPlaylistItems(playlist.Id);

            var playlistItemsHashSet = new HashSet<string>(playlistItems.Select(x => x.Id).ToList());
            foreach (var playlistItem in originalPlaylistItems)
            {
                if (!playlistItemsHashSet.Contains(playlistItem.Id))
                {
                    context.RemovePlaylistItem(playlistItem);
                    await callback(playlistItem, InsertStatus.Deleted, cancellationToken);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }
        }

        public async Task GetVideos(Func<IData, InsertStatus, CancellationToken, Task> callback, bool getAllVideos, CancellationToken cancellationToken)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            var playlistItems = await context.GetPlaylistItems();
            var videosToGet = playlistItems.Select(x => x.VideoId).ToList();
            var videosToSkip = getAllVideos ? new List<string>() : (await context.GetVideos()).Select(x => x.Id);
            videosToGet = videosToGet.Except(videosToSkip).ToList();
            await foreach (var video in _youTubeApi.GetVideos(videosToGet).WithCancellation(cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (video.IsDeletedFromYouTube)
                {
                    // We only want to insert this if we haven't already - because we want to preserve any existing data we have
                    if (!await context.VideoExists(video.Id))
                    {
                        await UpsertVideo(callback, video, cancellationToken);
                    }
                }
                else
                {
                    video.ThumbnailBytes = await GetThumbnail(cancellationToken, video);
                    await UpsertVideo(callback, video, cancellationToken);
                }
            }
        }

        private async Task<byte[]> GetThumbnail(CancellationToken cancellationToken, VideoData video)
        {
            try
            {
                return await _httpClientWrapper.GetByteArrayAsync(video.ThumbnailUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.Debug($"Video {video.DisplayInfo()} has no thumbnail, or we errored - Error: {ex}");
                return new byte[0];
            }
        }

        private async Task UpsertVideo(Func<IData, InsertStatus, CancellationToken, Task> callback, VideoData video, CancellationToken cancellationToken)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            var result = await context.UpsertVideo(video);
            await callback(video, result, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task GetUnicodeVideoTitles(Action<string> callback)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            var videoTitles = await context.GetVideoTitles();
            foreach (var videoTitle in videoTitles)
            {
                if (videoTitle.ToCharArray().Any(x => x > 1000))
                {
                    callback(videoTitle);
                }
            }
        }

        public async Task<PlaylistItemData> AddVideoToPlaylist(string playlistId, string videoId)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            var playlistItem = await _youTubeApi.AddVideoToPlaylist(playlistId, videoId);
            await context.UpsertPlaylistItem(playlistItem);
            await context.SaveChangesAsync();
            return playlistItem;
        }

        public async Task RemoveVideoFromPlaylist(string playlistId, string videoId)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            var playlistItem = await context.GetPlaylistItem(playlistId, videoId);
            if (playlistItem == null)
                return;

            await _youTubeApi.RemoveVideoFromPlaylist(playlistItem.Id);
            context.RemovePlaylistItem(playlistItem);
            await context.SaveChangesAsync();
        }

        private async Task RemovePlaylist(string playlistId)
        {
            var context = _youTubeCleanupToolDbContextFactory.Create();
            context.RemovePlaylist(playlistId);
            await context.SaveChangesAsync();
        }
    }
}
