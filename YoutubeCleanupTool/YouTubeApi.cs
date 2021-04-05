using System;
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
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.YouTube.v3.Data;
using YouTubeApiWrapper.Interfaces;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeApiWrapper
{
    public class YouTubeApi : IYouTubeApi
    {
        private readonly IMapper _mapper;
        private readonly IAppSettings _appSettings;
        private readonly YouTubeServiceCreatorOptions _youTubeServiceCreatorOptions;
        private IYouTubeServiceWrapper _youTubeServiceWrapper;

        public YouTubeApi([NotNull] IMapper mapper,
            [NotNull] IAppSettings appSettings,
            [NotNull] YouTubeServiceCreatorOptions youTubeServiceCreatorOptions)
        {
            _mapper = mapper;
            _appSettings = appSettings;
            _youTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
        }

        public async IAsyncEnumerable<PlaylistData> GetPlaylists()
        {
            var playlists = await HandleSecretRevocation(async getNewToken => await (await CreateYouTubeService(getNewToken)).GetPlaylists());
            
            foreach (var playlist in playlists)
            {
                yield return _mapper.Map<PlaylistData>(playlist);
            }
        }

        public async IAsyncEnumerable<PlaylistItemData> GetPlaylistItems(string playlistId, Func<string, Task> playlistGotDeleted)
        {
            List<PlaylistItem> playlistItems = null;
            try
            {
                playlistItems = await HandleSecretRevocation(async getNewToken => await (await CreateYouTubeService(getNewToken)).GetPlaylistItems(playlistId));
            }
            catch (Google.GoogleApiException ex)
            {
                if (ex.Message.ContainsCi("[playlistNotFound]"))
                {
                    await playlistGotDeleted(playlistId);
                }
                else
                {
                    throw;
                }
            }

            foreach (var item in playlistItems ?? new List<PlaylistItem>())
            {
                yield return _mapper.Map<PlaylistItemData>(item);
            }
        }

        public async IAsyncEnumerable<VideoData> GetVideos(List<string> videoIdsToGet)
        {
            foreach (var videoId in videoIdsToGet)
            {
                var video = (await HandleSecretRevocation(async getNewToken
                    => await (await CreateYouTubeService(getNewToken)).GetVideos(videoId))).FirstOrDefault();

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

        public async Task<PlaylistItemData> AddVideoToPlaylist(string playlistId, string videoId)
        {
            return await HandleSecretRevocation(async getNewToken =>
            {
                var playlistItem = await (await CreateYouTubeService(getNewToken)).AddVideoToPlaylist(playlistId, videoId);
                return _mapper.Map<PlaylistItemData>(playlistItem);
            });
        }

        public async Task RemoveVideoFromPlaylist(string playlistItemId)
        {
            await HandleSecretRevocation(async getNewToken =>
            {
                await (await CreateYouTubeService(getNewToken)).RemoveVideoFromPlaylist(playlistItemId);
                return Task.CompletedTask;
            });
        }

        private async Task<T> HandleSecretRevocation<T>(Func<bool, Task<T>> methodWhichCouldResultInNoAuthentication)
        {
            try
            {
                return await methodWhichCouldResultInNoAuthentication(false);
            }
            catch (TokenResponseException)
            {
                return await methodWhichCouldResultInNoAuthentication(true);
            }
        }
        
        private async Task<IYouTubeServiceWrapper> CreateYouTubeService(bool getNewToken)
        {
            if (_youTubeServiceWrapper != null && !getNewToken)
                return _youTubeServiceWrapper;

            // TODO: if requesting new scopes, delete %appdata%\_youTubeServiceCreatorOptions.FileDataStoreName\.* - this is where the refresh/accesstoken/scopes are stored
            UserCredential credential;
            using (var stream = new FileStream(_youTubeServiceCreatorOptions.ClientSecretPath, FileMode.Open, FileAccess.Read))
            {
                var installedApp = new AuthorizationCodeInstalledApp(
                    new GoogleAuthorizationCodeFlow(
                        new GoogleAuthorizationCodeFlow.Initializer
                        {
                            ClientSecrets = GoogleClientSecrets.Load(stream).Secrets,
                            // Console app should only need ReadOnly. the UI needs write access
                            Scopes = new List<string> { YouTubeService.Scope.YoutubeReadonly, YouTubeService.Scope.Youtube },
                            DataStore = new FileDataStore(_youTubeServiceCreatorOptions.FileDataStoreName)
                        }),
                        new LocalServerCodeReceiver());
                credential = await installedApp.AuthorizeAsync("user", CancellationToken.None);
            }

            if (getNewToken)
            {
                await credential.RefreshTokenAsync(CancellationToken.None);
            }

            // Create the service.
            _youTubeServiceWrapper = new YouTubeServiceWrapper(new BaseClientService.Initializer()
            {
                ApiKey = _appSettings.ApiKey,
                HttpClientInitializer = credential,
                ApplicationName = "YouTube cleanup tool",
            });
            return _youTubeServiceWrapper;
        }
    }
}