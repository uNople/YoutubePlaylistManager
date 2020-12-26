using AutoMapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeCleanupTool.Domain;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class YouTubeApi : IYouTubeApi
    {
        private readonly IMapper _mapper;
        private readonly ICredentialManagerWrapper _credentialManagerWrapper;
        private readonly YoutubeServiceCreatorOptions _youtubeServiceCreatorOptions;

        public YouTubeApi([NotNull] IMapper mapper,
            [NotNull] ICredentialManagerWrapper credentialManagerWrapper,
            [NotNull] YoutubeServiceCreatorOptions youtubeServiceCreatorOptions)
        {
            _mapper = mapper;
            _credentialManagerWrapper = credentialManagerWrapper;
            _youtubeServiceCreatorOptions = youtubeServiceCreatorOptions;
        }

        public async IAsyncEnumerable<PlaylistData> GetPlaylists()
        {
            var playlists = await GetYouYubeWrapper().GetPlaylists();
            foreach (var playlist in playlists)
            {
                yield return _mapper.Map<PlaylistData>(playlist);
            }    
        }

        public async IAsyncEnumerable<PlaylistItemData> GetPlaylistItems(List<PlaylistData> playlists)
        {
            foreach (var playlist in playlists)
            {
                var playlistItems = await GetYouYubeWrapper().GetPlaylistItems(playlist.Id);
                foreach (var playlistItem in playlistItems)
                {
                    yield return _mapper.Map<PlaylistItemData>(playlistItem);
                }
            }
        }

        public async IAsyncEnumerable<VideoData> GetVideos(List<string> videoIdsToGet)
        {
            foreach (var videoId in videoIdsToGet)
            {
                var video = (await GetYouYubeWrapper().GetVideos(videoId)).FirstOrDefault();
                if (video == null)
                    continue;

                yield return _mapper.Map<VideoData>(video);
            }
        }

        private IYouTubeServiceWrapper GetYouYubeWrapper()
        {
            return CreateYouTubeService().GetAwaiter().GetResult();
        }

        public async Task<IYouTubeServiceWrapper> CreateYouTubeService()
        {
            var apiKey = _credentialManagerWrapper.GetApiKey();
            UserCredential credential;
            using (var stream = new FileStream(_youtubeServiceCreatorOptions.ClientSecretPath, FileMode.Open, FileAccess.Read))
            {
                var installedApp = new AuthorizationCodeInstalledApp(
                    new GoogleAuthorizationCodeFlow(
                        new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = GoogleClientSecrets.Load(stream).Secrets,
                            Scopes = new List<string> { YouTubeService.Scope.YoutubeReadonly },
                            DataStore = new FileDataStore(_youtubeServiceCreatorOptions.FileDataStoreName)
                        }),
                        new LocalServerCodeReceiver());
                credential = await installedApp.AuthorizeAsync("user", CancellationToken.None);
            }

            // Create the service.
            var service = new YouTubeServiceWrapper(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                HttpClientInitializer = credential,
                ApplicationName = "Youtube cleanup tool",
            });
            apiKey = null;
            return service;
        }
    }
}