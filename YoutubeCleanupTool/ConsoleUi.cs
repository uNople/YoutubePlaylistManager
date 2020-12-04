using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    // TODO: Move basically everything out of here - it's just here temporarily... """"temporarily""""
    internal class ConsoleUi : IConsoleUi
    {
        private readonly IYouTubeServiceWrapper _youTubeServiceWrapper;
        private readonly IPersister _persister;

        public ConsoleUi(IYouTubeServiceWrapper youTubeServiceWrapper, IPersister persister)
        {
            _youTubeServiceWrapper = youTubeServiceWrapper ?? throw new ArgumentNullException(nameof(youTubeServiceWrapper));
            _persister = persister ?? throw new ArgumentNullException(nameof(persister));
        }

        public void Run()
        {
            Task.WaitAll(Execute());
        }

        private async Task Execute()
        {
            var forceGetAll = false;

            var playlists = await _youTubeServiceWrapper.GetPlaylists(forceGetAll);

            const string playlistItemFile = "playlistItems.json";
            var cachedPlaylistItems = new Dictionary<string, List<PlaylistItem>>();
            if (!forceGetAll && File.Exists(playlistItemFile))
            {
                cachedPlaylistItems = _persister.GetData<Dictionary<string, List<PlaylistItem>>>(playlistItemFile);
            }

            foreach (var playlist in playlists)
            {
                if (!cachedPlaylistItems.ContainsKey(playlist.Id))
                {
                    var playlistItems = await _youTubeServiceWrapper.GetPlaylistItems(playlist.Id);
                    cachedPlaylistItems.Add(playlist.Id, playlistItems);
                    _persister.SaveData(playlistItemFile, cachedPlaylistItems);
                }
            }

            const string videosFile = "videosFile.json";

            var videos = new List<Video>();
            var videosThatExist = new HashSet<string>();
            if (!forceGetAll && File.Exists(videosFile))
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
                    var video = await _youTubeServiceWrapper.GetVideos(playlistItem.ContentDetails.VideoId);
                    Console.Write(".");
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
        }
    }
}