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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeCleanupTool
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.WaitAll(Execute());
        }

        private static async Task Execute()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
            };

            var youTubeService = await new YoutubeService().CreateYouTubeService("googleapikey", @"C:\Users\unopl\source\repos\Creds\client_secret.json", "Youtube.Api.Storage");

            const string playlistFile = "playlists.json";
            List<Playlist> playlists;
            if (File.Exists(playlistFile))
            {
                playlists = JsonConvert.DeserializeObject<List<Playlist>>(File.ReadAllText(playlistFile));
            }
            else
            {
                playlists = await GetPlaylists(youTubeService);
                var serialized = JsonConvert.SerializeObject(playlists);
                File.WriteAllText(playlistFile, serialized);
            }
        }

        private static async Task<List<Playlist>> GetPlaylists(YouTubeService service)
        {
            var playlistRequest = service.Playlists.List("snippet");
            playlistRequest.Mine = true;
            // NOTE: it's limited to 50 by google
            playlistRequest.MaxResults = 50;
            var playlists = new List<Playlist>();

            // TODO: error handling
            var playlistResponse = await playlistRequest.ExecuteAsync();
            playlists.AddRange(playlistResponse.Items);

            while (playlistResponse.NextPageToken != null)
            {
                playlistRequest.PageToken = playlistResponse.NextPageToken;
                playlistResponse = await playlistRequest.ExecuteAsync();
                playlists.AddRange(playlistResponse.Items);
            }

            return playlists;
        }
    }
}
