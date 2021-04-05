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

        public GetAndCacheYouTubeData([NotNull] IYouTubeApi youTubeApi,
            [NotNull] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            [NotNull] IHttpClientWrapper httpClientWrapper
            )
        {
            _youTubeApi = youTubeApi;
            _youTubeCleanupToolDbContextFactory = youTubeCleanupToolDbContextFactory;
            _httpClientWrapper = httpClientWrapper;
        }

        public async Task GetPlaylists(Func<IData, InsertStatus, CancellationToken, Task> callback, CancellationToken cancellationToken)
        {
            await foreach (var playlist in _youTubeApi.GetPlaylists().WithCancellation(cancellationToken))
            {
                var result = await _youTubeCleanupToolDbContextFactory.Create().UpsertPlaylist(playlist);
                await callback(playlist, result, cancellationToken);
            }
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync(cancellationToken);
        }

        public async Task GetPlaylistItems(Func<IData, InsertStatus, CancellationToken, Task> callback, CancellationToken cancellationToken)
        {
            var playlists = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylists();
            foreach (var playlist in playlists)
            {
                await GetPlaylistItems(callback, playlist, cancellationToken);
            }
            
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync(cancellationToken);
        }

        public async Task GetPlaylistItemsForPlaylist(Func<IData, InsertStatus, CancellationToken, Task> callback, PlaylistData playlist, CancellationToken cancellationToken)
        {
            await GetPlaylistItems(callback, playlist, cancellationToken);
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync(cancellationToken);
        }

        private async Task GetPlaylistItems(Func<IData, InsertStatus, CancellationToken, Task> callback, PlaylistData playlist, CancellationToken cancellationToken)
        {
            var playlistItems = new List<PlaylistItemData>();
            await foreach (var playlistItem in _youTubeApi.GetPlaylistItems(playlist.Id, RemovePlaylist).WithCancellation(cancellationToken))
            {
                playlistItems.Add(playlistItem);
                var result = await _youTubeCleanupToolDbContextFactory.Create().UpsertPlaylistItem(playlistItem);
                await callback(playlistItem, result, cancellationToken);
            }

            // Now we have to remove playlist items we didn't get back from the API - Otherwise if we delete + add items then we end up with duplicates
            var originalPlaylistItems = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylistItems(playlist.Id);

            var playlistItemsHashSet = new HashSet<string>(playlistItems.Select(x => x.Id).ToList());
            foreach (var playlistItem in originalPlaylistItems)
            {
                if (!playlistItemsHashSet.Contains(playlistItem.Id))
                {
                    _youTubeCleanupToolDbContextFactory.Create().RemovePlaylistItem(playlistItem);
                    await callback(playlistItem, InsertStatus.Deleted, cancellationToken);
                }
            }
        }

        public async Task GetVideos(Func<IData, InsertStatus, CancellationToken, Task> callback, bool getAllVideos, CancellationToken cancellationToken)
        {
            var playlistItems = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylistItems();
            var videosToGet = playlistItems.Select(x => x.VideoId).ToList();
            var videosToSkip = getAllVideos ? new List<string>() : (await _youTubeCleanupToolDbContextFactory.Create().GetVideos()).Select(x => x.Id);
            videosToGet = videosToGet.Except(videosToSkip).ToList();
            await foreach (var video in _youTubeApi.GetVideos(videosToGet).WithCancellation(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    await callback(new VideoData { Title = "CANCELLED", Id = "CANCELLED" }, InsertStatus.Inserted, cancellationToken);
                    return;
                }

                if (video.IsDeletedFromYouTube)
                {
                    // We only want to insert this if we haven't already - because we want to preserve any existing data we have
                    if (!await _youTubeCleanupToolDbContextFactory.Create().VideoExists(video.Id))
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
            catch
            {
                // TODO: Log?
                return new byte[0];
            }
        }

        private async Task UpsertVideo(Func<IData, InsertStatus, CancellationToken, Task> callback, VideoData video, CancellationToken cancellationToken)
        {
            var result = await _youTubeCleanupToolDbContextFactory.Create().UpsertVideo(video);
            await callback(video, result, cancellationToken);
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync(cancellationToken);
        }

        public async Task GetUnicodeVideoTitles(Action<string> callback)
        {
            var videoTitles = await _youTubeCleanupToolDbContextFactory.Create().GetVideoTitles();
            foreach (var videoTitle in videoTitles)
            {
                if (videoTitle.ToCharArray().Any(x => ((int)x) > 1000))
                {
                    callback(videoTitle);
                }
            }
        }

        public async Task<PlaylistItemData> AddVideoToPlaylist(string playlistId, string videoId)
        {
            var playlistItem = await _youTubeApi.AddVideoToPlaylist(playlistId, videoId);
            await _youTubeCleanupToolDbContextFactory.Create().UpsertPlaylistItem(playlistItem);
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync();
            return playlistItem;
        }

        public async Task RemoveVideoFromPlaylist(string playlistId, string videoId)
        {
            var playlistItem = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylistItem(playlistId, videoId);
            if (playlistItem == null)
                return;

            await _youTubeApi.RemoveVideoFromPlaylist(playlistItem.Id);
            _youTubeCleanupToolDbContextFactory.Create().RemovePlaylistItem(playlistItem);
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync();
        }

        public async Task RemovePlaylist(string playlistId)
        {
            _youTubeCleanupToolDbContextFactory.Create().RemovePlaylist(playlistId);
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync();
        }
    }
}
