using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    // TODO: Rename
    // TODO: This class mixes saving/getting data with 'pure' get the data. It's also inconsistent - some things read data first, and then
    // based off the result of that, it either gets more things, or doesn't get as much (eg, we don't get all videos each time, only new ones)
    public class WhereTheRubberHitsTheRoad : IWhereTheRubberHitsTheRoad
    {
        private readonly IYouTubeServiceCreator _youTubeServiceCreator;
        private readonly IPersister _persister;

        public WhereTheRubberHitsTheRoad(IYouTubeServiceCreator youTubeServiceCreator, IPersister persister)
        {
            _youTubeServiceCreator = youTubeServiceCreator ?? throw new ArgumentNullException(nameof(youTubeServiceCreator));
            _persister = persister ?? throw new ArgumentNullException(nameof(persister));
        }

        public async Task<List<Playlist>> GetPlaylists()
        {
            const string playlistFile = "playlists.json";
            // TODO: Make less ugly
            var playlists = await (await _youTubeServiceCreator.YouTubeServiceWrapper.Value).GetPlaylists();
            _persister.SaveData(playlistFile, playlists);
            return playlists;
        }

        public async Task<Dictionary<string, List<PlaylistItem>>> GetPlaylistItems(List<Playlist> playlists)
        {
            const string playlistItemFile = "playlistItems.json";
            var cachedPlaylistItems = new Dictionary<string, List<PlaylistItem>>();
            if (_persister.DataExists(playlistItemFile))
            {
                cachedPlaylistItems = _persister.GetData<Dictionary<string, List<PlaylistItem>>>(playlistItemFile);
            }

            foreach (var playlist in playlists)
            {
                if (!cachedPlaylistItems.ContainsKey(playlist.Id))
                {
                    var playlistItems = await (await _youTubeServiceCreator.YouTubeServiceWrapper.Value).GetPlaylistItems(playlist.Id);
                    cachedPlaylistItems.Add(playlist.Id, playlistItems);
                    _persister.SaveData(playlistItemFile, cachedPlaylistItems);
                }
            }

            return cachedPlaylistItems;
        }

        public async Task<List<Video>> GetVideos(Dictionary<string, List<PlaylistItem>> cachedPlaylistItems)
        {
            const string videosFile = "videosFile.json";

            var videos = new List<Video>();
            var videosThatExist = new HashSet<string>();
            if (_persister.DataExists(videosFile))
            {
                videos = _persister.GetData<List<Video>>(videosFile);
                videosThatExist = new HashSet<string>(videos.Select(x => x.Id));
            }

            const int saveEvery = 10;
            var current = 0;
            foreach (var item in cachedPlaylistItems)
            {
                foreach (var playlistItem in item.Value)
                {
                    if (videosThatExist.Contains(playlistItem.ContentDetails.VideoId))
                        continue;

                    current++;
                    var video = await (await _youTubeServiceCreator.YouTubeServiceWrapper.Value).GetVideos(playlistItem.ContentDetails.VideoId);
                    foreach (var videoData in video)
                    {
                        videos.Add(videoData);
                        videosThatExist.Add(videoData.Id);
                    }

                    if (current % saveEvery == 0)
                    {
                        _persister.SaveData(videosFile, videos);
                    }
                }
            }
            _persister.SaveData(videosFile, videos);

            return videos;
        }
    }
}