using AdysTech.CredentialManager;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security;
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
                var credential = new NetworkCredential("", CredentialManager.GetICredential("googleapikey").CredentialBlob);
                var response = await httpClient.GetAsync($"https://www.googleapis.com/youtube/v3/playlists?maxResults=500&prettyPrint=true&channelId=UCsDyfu6em80Ug1T59poKt2Q&key={credential.Password}");
                var result = await response.Content.ReadAsStringAsync();
                var parsedResult = JsonConvert.DeserializeObject<JObject>(result);

                var playlistItems = new List<Playlist>();
                foreach (var item in parsedResult)
                {
                    if (item.Key == "items")
                    {
                        playlistItems = item.Value.ToObject<List<Playlist>>();
                    }
                }
            }
        }

        [DebuggerDisplay("{Id}")]
        class Playlist
        {
            public string Kind { get; set; }
            public string Etag { get; set; }
            public string Id { get; set; }
        }
    }
}
