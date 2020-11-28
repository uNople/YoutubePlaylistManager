using AdysTech.CredentialManager;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.YouTube.v3.ActivitiesResource;

namespace YoutubeCleanupTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.WaitAll(Execute());
        }

        private static YouTubeService _youTubeService;

        private static async Task Execute()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            _youTubeService = await new YoutubeService().CreateYouTubeService("googleapikey", @"C:\Users\unopl\source\repos\Creds\client_secret.json", "Youtube.Api.Storage");

            var playlists = await GetPlaylists(true);

            const string playlistItemFile = "playlistItems.json";
            var cachedPlaylistItems = new Dictionary<string, List<PlaylistItem>>();
            if (File.Exists(playlistItemFile))
            {
                cachedPlaylistItems = JsonConvert.DeserializeObject<Dictionary<string, List<PlaylistItem>>>(File.ReadAllText(playlistItemFile));
            }

            foreach (var playlist in playlists)
            {
                if (!cachedPlaylistItems.ContainsKey(playlist.Id))
                {
                    var playlistItems = await GetPlaylistItems(playlist.Id);
                    cachedPlaylistItems.Add(playlist.Id, playlistItems);
                    File.WriteAllText(playlistItemFile, JsonConvert.SerializeObject(cachedPlaylistItems));
                }
                else
                {
                    // update the cached version of these playlist items with new ones
                    // This is a 'force re-get'
                }
            }
        }

        private static async Task<List<PlaylistItem>> GetPlaylistItems(string playlistId)
        {
            // https://developers.google.com/youtube/v3/docs/playlistItems
            // playlist LL and LM to get liked videos / liked music
            var playlistItems = _youTubeService.PlaylistItems.List("contentDetails,id,snippet,status");
            playlistItems.PlaylistId = playlistId;
            return await YouTubeServiceRequestWrapper.GetResults<PlaylistItem>(playlistItems);
        }

        private static async Task<List<Playlist>> GetPlaylists(bool forceGet)
        {
            const string playlistFile = "playlists.json";
            if (!forceGet && File.Exists(playlistFile))
            {
                return JsonConvert.DeserializeObject<List<Playlist>>(File.ReadAllText(playlistFile));
            }

            // auditDetails requires youtubepartner-channel-audit scope
            // brandingSettings, contentOwnerDetails requires something?
            // statistics topicDetails
            // Don't care about: localizations (even though I can get it)
            var playlistRequest = _youTubeService.Playlists.List("contentDetails,id,snippet,status");
            playlistRequest.Mine = true;
            var result = await YouTubeServiceRequestWrapper.GetResults<Playlist>(playlistRequest);

            // force-get LL and LM playlists
            playlistRequest = _youTubeService.Playlists.List("contentDetails,id,snippet,status");
            playlistRequest.Id = "LL";
            result.AddRange(await YouTubeServiceRequestWrapper.GetResults<Playlist>(playlistRequest));
            playlistRequest.Id = "LM";
            result.AddRange(await YouTubeServiceRequestWrapper.GetResults<Playlist>(playlistRequest));

            var serialized = JsonConvert.SerializeObject(result);
            File.WriteAllText(playlistFile, serialized);

            return result;
        }
    }
}
