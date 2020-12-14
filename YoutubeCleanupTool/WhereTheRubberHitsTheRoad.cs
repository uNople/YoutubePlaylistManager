using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YoutubeCleanupTool.Interfaces;
using YoutubeCleanupTool.Model;

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
            var playlists = await GetYouYubeWrapper().GetPlaylists();
            _persister.SaveData(SavePathNames.PlaylistFile, playlists);
            return playlists;
        }

        public async Task<List<PlaylistItem>> GetPlaylistItems(List<Playlist> playlists)
        {

            var playlistItemData = new List<PlaylistItem>();
            foreach (var playlist in playlists)
            {
                var playlistItems = await GetYouYubeWrapper().GetPlaylistItems(playlist.Id);
                playlistItemData.AddRange(playlistItems);
            }
            _persister.SaveData(SavePathNames.PlaylistItemFile, playlistItemData);

            return playlistItemData;
        }

        public async IAsyncEnumerable<Video> GetVideos(List<PlaylistItem> cachedPlaylistItems)
        {

            // TODO: do I actually want to get only if it doesn't exist?
            var videos = new List<Video>();
            var videosThatExist = new HashSet<string>();
            if (_persister.DataExists(SavePathNames.VideosFile))
            {
                videos = _persister.GetData<List<Video>>(SavePathNames.VideosFile);
                videosThatExist = new HashSet<string>(videos.Select(x => x.Id));
            }

            const int saveEvery = 10;
            var current = 0;
            foreach (var playlistItem in cachedPlaylistItems)
            {
                if (videosThatExist.Contains(playlistItem.ContentDetails.VideoId))
                    continue;

                current++;
                var video = (await GetYouYubeWrapper().GetVideos(playlistItem.ContentDetails.VideoId)).FirstOrDefault();
                if (video == null)
                    continue;

                videos.Add(video);
                videosThatExist.Add(video.Id);
                yield return video;

                if (current % saveEvery == 0)
                {
                    _persister.SaveData(SavePathNames.VideosFile, videos);
                }
            }
            _persister.SaveData(SavePathNames.VideosFile, videos);
        }

        private IYouTubeServiceWrapper GetYouYubeWrapper()
        {
            return _youTubeServiceCreator.YouTubeServiceWrapper.Value.GetAwaiter().GetResult();
        }
    }
}