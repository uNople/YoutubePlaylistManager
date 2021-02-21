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

        public async Task GetPlaylists(Action<PlaylistData, InsertStatus> callback)
        {
            await foreach (var playlist in _youTubeApi.GetPlaylists())
            {
                var result = await _youTubeCleanupToolDbContextFactory.Create().UpsertPlaylist(playlist);
                callback(playlist, result);
            }
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync();
        }

        public async Task GetPlaylistItems(Action<PlaylistItemData, InsertStatus> callback)
        {
            var playlists = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylists();
            foreach (var playlist in playlists)
            {
                await GetPlaylistItems(callback, playlist);
            }
            
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync();
        }

        public async Task GetPlaylistItemsForPlaylist(Action<PlaylistItemData, InsertStatus> callback, PlaylistData playlist)
        {
            await GetPlaylistItems(callback, playlist);
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync();
        }

        private async Task GetPlaylistItems(Action<PlaylistItemData, InsertStatus> callback, PlaylistData playlist)
        {
            var playlistItems = new List<PlaylistItemData>();
            await foreach (var playlistItem in _youTubeApi.GetPlaylistItems(playlist.Id, RemovePlaylist))
            {
                playlistItems.Add(playlistItem);
                var result = await _youTubeCleanupToolDbContextFactory.Create().UpsertPlaylistItem(playlistItem);
                callback(playlistItem, result);
            }

            // Now we have to remove playlist items we didn't get back from the API - Otherwise if we delete + add items then we end up with duplicates
            var originalPlaylistItems = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylistItems(playlist.Id);

            var playlistItemsHashSet = new HashSet<string>(playlistItems.Select(x => x.Id).ToList());
            foreach (var playlistItem in originalPlaylistItems)
            {
                if (!playlistItemsHashSet.Contains(playlistItem.Id))
                {
                    _youTubeCleanupToolDbContextFactory.Create().RemovePlaylistItem(playlistItem);
                    callback(playlistItem, InsertStatus.Deleted);
                }
            }
        }

        public async Task GetVideos(Action<VideoData, InsertStatus> callback, bool getAllVideos, CancellationToken cancellationToken)
        {
            var playlistItems = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylistItems();
            var videosToGet = playlistItems.Select(x => x.VideoId).ToList();
            var videosToSkip = getAllVideos ? new List<string>() : (await _youTubeCleanupToolDbContextFactory.Create().GetVideos()).Select(x => x.Id);
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
                    if (!await _youTubeCleanupToolDbContextFactory.Create().VideoExists(video.Id))
                    {
                        await UpsertVideo(callback, video);
                    }
                }
                else
                {
                    video.ThumbnailBytes = await GetThumbnail(cancellationToken, video);
                    await UpsertVideo(callback, video);
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

        private async Task UpsertVideo(Action<VideoData, InsertStatus> callback, VideoData video)
        {
            var result = await _youTubeCleanupToolDbContextFactory.Create().UpsertVideo(video);
            callback(video, result);
            await _youTubeCleanupToolDbContextFactory.Create().SaveChangesAsync();
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
