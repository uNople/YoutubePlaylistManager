using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Domain
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

        public async Task GetNewVideos(Action<VideoData, InsertStatus> callback)
        {
            var playlistItems = (await _youTubeCleanupToolDbContext.GetPlaylistItems()).Select(x => x.VideoId).ToList();
            var videos = (await _youTubeCleanupToolDbContext.GetVideos()).Select(x => x.Id);
            await foreach (var video in _youTubeApi.GetVideos(playlistItems.Except(videos).ToList()))
            {
                var result = await _youTubeCleanupToolDbContext.UpsertVideo(video);
                callback(video, result);
                await _youTubeCleanupToolDbContext.SaveChangesAsync();
            }
        }
    }
}
