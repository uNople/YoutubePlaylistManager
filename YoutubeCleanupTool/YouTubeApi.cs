using AutoMapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YouTubeApiWrapper.Interfaces;
using YouTubeCleanupTool;
using YouTubeCleanupTool.Domain;

namespace YouTubeApiWrapper
{
    public class YouTubeApi : IYouTubeApi
    {
        private readonly IMapper _mapper;
        private readonly ICredentialManagerWrapper _credentialManagerWrapper;
        private readonly YouTubeServiceCreatorOptions _youTubeServiceCreatorOptions;
        private IYouTubeServiceWrapper _youTubeServiceWrapper;

        public YouTubeApi([NotNull] IMapper mapper,
            [NotNull] ICredentialManagerWrapper credentialManagerWrapper,
            [NotNull] YouTubeServiceCreatorOptions youTubeServiceCreatorOptions)
        {
            _mapper = mapper;
            _credentialManagerWrapper = credentialManagerWrapper;
            _youTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
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
                {
                    yield return new VideoData { Id = videoId, Title = "deleted", IsDeletedFromYouTube = true };
                }
                else
                {
                    yield return _mapper.Map<VideoData>(video);
                }
            }
        }

        private IYouTubeServiceWrapper GetYouYubeWrapper()
        {
            return CreateYouTubeService().GetAwaiter().GetResult();
        }

        public async Task<IYouTubeServiceWrapper> CreateYouTubeService()
        {
            if (_youTubeServiceWrapper != null)
                return _youTubeServiceWrapper;

            var apiKey = _credentialManagerWrapper.GetApiKey();
            UserCredential credential;
            using (var stream = new FileStream(_youTubeServiceCreatorOptions.ClientSecretPath, FileMode.Open, FileAccess.Read))
            {
                var installedApp = new AuthorizationCodeInstalledApp(
                    new GoogleAuthorizationCodeFlow(
                        new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = GoogleClientSecrets.Load(stream).Secrets,
                            Scopes = new List<string> { YouTubeService.Scope.YoutubeReadonly },
                            DataStore = new FileDataStore(_youTubeServiceCreatorOptions.FileDataStoreName)
                        }),
                        new LocalServerCodeReceiver());
                credential = await installedApp.AuthorizeAsync("user", CancellationToken.None);
            }

            // Create the service.
            _youTubeServiceWrapper = new YouTubeServiceWrapper(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                HttpClientInitializer = credential,
                ApplicationName = "Youtube cleanup tool",
            });
            apiKey = null;
            return _youTubeServiceWrapper;
        }
    }
}