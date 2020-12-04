using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class YouTubeServiceWrapper : YouTubeService, IYouTubeServiceWrapper
    {
        private readonly IPersister _persister;
        public YouTubeServiceWrapper(IPersister persister, Initializer initializer) : base(initializer)
        {
            _persister = persister ?? throw new ArgumentNullException(nameof(persister));
        }

        public async Task<List<Video>> GetVideos(string id)
        {
            // https://developers.google.com/youtube/v3/docs/videos/list
            // playlist LL and LM to get liked videos / liked music
            var items = Videos.List("contentDetails,id,snippet,status,player,projectDetails,recordingDetails,statistics,topicDetails");
            items.Id = id;
            return await YouTubeServiceRequestWrapper.GetResults<Video>(items);
        }

        public async Task<List<PlaylistItem>> GetPlaylistItems(string playlistId)
        {
            // https://developers.google.com/youtube/v3/docs/playlistItems/list
            var playlistItems = PlaylistItems.List("contentDetails,id,snippet,status");
            playlistItems.PlaylistId = playlistId;
            return await YouTubeServiceRequestWrapper.GetResults<PlaylistItem>(playlistItems);
        }

        public async Task<List<Playlist>> GetPlaylists(bool forceGet)
        {
            const string playlistFile = "playlists.json";
            if (!forceGet && _persister.DataExists(playlistFile))
            {
                return _persister.GetData<List<Playlist>>(playlistFile);
            }

            // auditDetails requires youtubepartner-channel-audit scope
            // brandingSettings, contentOwnerDetails requires something?
            // statistics topicDetails
            // Don't care about: localizations (even though I can get it)
            var playlistRequest = Playlists.List("contentDetails,id,snippet,status");
            playlistRequest.Mine = true;
            var result = await YouTubeServiceRequestWrapper.GetResults<Playlist>(playlistRequest);

            // force-get LL and LM playlists
            playlistRequest = Playlists.List("contentDetails,id,snippet,status");
            playlistRequest.Id = "LL";
            result.AddRange(await YouTubeServiceRequestWrapper.GetResults<Playlist>(playlistRequest));
            playlistRequest.Id = "LM";
            result.AddRange(await YouTubeServiceRequestWrapper.GetResults<Playlist>(playlistRequest));

            _persister.SaveData(playlistFile, result);
            
            return result;
        }
    }
}
