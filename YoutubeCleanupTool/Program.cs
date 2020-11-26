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
            using (var httpClient = new HttpClient())
            {
                // TODO:
                // Get credentials with name googleapikey
                // make requests to youtube/youtube music api with key=API_KEY
                var apiKey = CredentialManager.GetICredential("googleapikey").UserName;
                UserCredential credential;
                using (var stream = new FileStream(@"C:\Users\unopl\source\repos\Creds\client_secret.json", FileMode.Open, FileAccess.Read))
                {
                    credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        new[] { YouTubeService.Scope.YoutubeReadonly },
                        "user", CancellationToken.None, new FileDataStore("Youtube.Api.Storage"));
                }

                // Create the service.
                var service = new YouTubeService(new BaseClientService.Initializer()
                {
                    ApiKey = apiKey,
                    HttpClientInitializer = credential,
                    ApplicationName = "Youtube cleanup tool",
                });
                apiKey = null;

                var playlistRequest = service.Playlists.List("snippet");
                playlistRequest.Mine = true;
                // NOTE: it's limited to 50 by google
                playlistRequest.MaxResults = 50;
                var playlistResponse = await playlistRequest.ExecuteAsync();
                var playlists = new List<Playlist>();
                playlists.AddRange(playlistResponse.Items);

                while (playlistResponse.NextPageToken != null)
                {
                    playlistRequest.PageToken = playlistResponse.NextPageToken;
                    playlistResponse = await playlistRequest.ExecuteAsync();
                    playlists.AddRange(playlistResponse.Items);
                }

            }
        }
    }
}
