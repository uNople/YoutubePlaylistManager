using AdysTech.CredentialManager;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class YouTubeServiceCreator : IYouTubeServiceCreator
    {
        private readonly ICredentialManagerWrapper _credentialManagerWrapper;
        private readonly YoutubeServiceCreatorOptions _youtubeServiceCreatorOptions;
        private readonly IPersister _persister;

        public YouTubeServiceCreator(ICredentialManagerWrapper credentialManagerWrapper, YoutubeServiceCreatorOptions youtubeServiceCreatorOptions, IPersister persister)
        {
            _credentialManagerWrapper = credentialManagerWrapper ?? throw new ArgumentNullException(nameof(credentialManagerWrapper));
            _youtubeServiceCreatorOptions = youtubeServiceCreatorOptions ?? throw new ArgumentNullException(nameof(youtubeServiceCreatorOptions));
            _persister = persister ?? throw new ArgumentNullException(nameof(persister));
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
            var service = new YouTubeServiceWrapper(_persister, new BaseClientService.Initializer()
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
