using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeCleanupTool;
using YoutubeCleanupTool.DataAccess;
using YoutubeCleanupTool.Interfaces;
using YoutubeCleanupTool.Model;
using YoutubeCleanupTool.Utils;

namespace YoutubeCleanupConsole
{
    public class ConsoleUi : IConsoleUi
    {
        private readonly ConsoleDisplayParams _consoleDisplayParams;
        private readonly IWhereTheRubberHitsTheRoad _whereTheRubberHitsTheRoad;
        private readonly ICredentialManagerWrapper _credentialManagerWrapper;
        private readonly IPersister _persister;
        private readonly YoutubeServiceCreatorOptions _youtubeServiceCreatorOptions;
        private readonly IYoutubeCleanupToolDbContext _youtubeCleanupToolDbContext;

        public ConsoleUi(
            [NotNull] ConsoleDisplayParams consoleDisplayParams,
            [NotNull] IWhereTheRubberHitsTheRoad whereTheRubberHitsTheRoad,
            [NotNull] ICredentialManagerWrapper credentialManagerWrapper,
            [NotNull] IPersister persister,
            [NotNull] YoutubeServiceCreatorOptions youtubeServiceCreatorOptions,
            [NotNull] IYoutubeCleanupToolDbContext youtubeCleanupToolDbContext)
        {
            _consoleDisplayParams = consoleDisplayParams;
            _whereTheRubberHitsTheRoad = whereTheRubberHitsTheRoad;
            _credentialManagerWrapper = credentialManagerWrapper;
            _persister = persister;
            _youtubeServiceCreatorOptions = youtubeServiceCreatorOptions;
            _youtubeCleanupToolDbContext = youtubeCleanupToolDbContext;
        }

        public async Task Run()
        {
            var commands = new Dictionary<string, Func<Task>>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "UpdateApiKey", async () => await Task.Run(() => _credentialManagerWrapper.PromptForKey()) },
                { "GetPlaylists", async () => await GetPlaylists() },
                { "GetPlaylistItems", async () => await Task.Run(async () => await GetPlaylistItems()) },
                { "GetVideos", async () => await Task.Run(async () => await GetVideos()) },
            };

            PromptForKeyIfNotExists();

            CheckClientJsonExists();

            while (true)
            {

                Draw(commands.Keys.ToList());

                var command = Console.ReadLine();

                if (commands.TryGetValue(command, out var func))
                {
                    try
                    {
                        await func();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error running command {command}. Error: {ex}");
                        PromptToContinue();
                    }
                }
            }
        }

        private static void PromptToContinue()
        {
            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();
        }

        private async Task GetPlaylists()
        {
            Console.WriteLine("Playlist Details:");
            var playlists = await _whereTheRubberHitsTheRoad.GetPlaylists();

            // TODO: use automapper or something
            var translatedPlaylists = playlists.Select(x => new PlaylistData
            {
                Title = x.Snippet.Localized.Title,
                Id = x.Id,
                PrivacyStatus = x.Status.PrivacyStatus,
                Kind = x.Kind,
                ThumbnailUrl = x.Snippet.Thumbnails.Default__.Url,
            }).ToList();

            // TODO: refactor out to a store or something. Would need to update entity if exists (or leave it) instead
            var currentItems = _youtubeCleanupToolDbContext.Playlists.ToList();
            _youtubeCleanupToolDbContext.Playlists.RemoveRange(currentItems);
            _youtubeCleanupToolDbContext.Playlists.AddRange(translatedPlaylists);
            await _youtubeCleanupToolDbContext.SaveChangesAsync();

            playlists
                .ForEach(x => Console.WriteLine($"{x.Id} - {x.Snippet.Title}"));
        }

        private async Task<List<PlaylistItem>> GetPlaylistItems()
        {
            var playlists = _persister.GetData<List<Playlist>>(SavePathNames.PlaylistFile);
            if (!playlists.Any())
            {
                playlists = await _whereTheRubberHitsTheRoad.GetPlaylists();
            }

            var playlistItems = (await _whereTheRubberHitsTheRoad.GetPlaylistItems(await _whereTheRubberHitsTheRoad.GetPlaylists()));

            Console.WriteLine("Playlist Item Details:");
            playlistItems.ForEach(x => Console.WriteLine($"{x.Id} - {x.Snippet.Title}"));
            return playlistItems;
        }

        private async Task GetVideos()
        {
            var playlistItems = _persister.GetData<List<PlaylistItem>>(SavePathNames.PlaylistItemFile);
            if (!playlistItems.Any())
            {
                playlistItems = await GetPlaylistItems();
            }

            Console.WriteLine("Video Details:");
            await foreach (var video in _whereTheRubberHitsTheRoad.GetVideos(playlistItems))
            {
                Console.WriteLine($"{video.Id} - {video.Snippet.Title}");
            }
        }

        // TODO: Move these Check Credential things into another class - but... maybe don't need to move it?
        // TODO: Write tests
        private void CheckClientJsonExists()
        {
            while (!_persister.DataExists(_youtubeServiceCreatorOptions.ClientSecretPath))
            {
                Console.WriteLine("Please download your client secret from google. URL should be: https://console.developers.google.com/?pli=1");
                Console.WriteLine($"Place client.json in {_youtubeServiceCreatorOptions.ClientSecretPath}");
                PromptToContinue();
            }
        }

        private void PromptForKeyIfNotExists()
        {
            if (!_credentialManagerWrapper.Exists())
            {
                _credentialManagerWrapper.PromptForKey();
            }
        }

        private void Draw(List<string> commands)
        {
            int commandIndex = 0;

            for (int line = 0; line < _consoleDisplayParams.Lines; line++)
            {
                if (!IsCommandLines(line) && !IsBorderLine(line))
                {
                    if (commandIndex < commands.Count)
                    {
                        Console.Write($" >   {commands[commandIndex++]}");
                    }
                }
                for (int column = 0; column < _consoleDisplayParams.Columns; column++)
                {
                    if (IsCommandLines(line))
                    {
                        // TODO: Please enter a command
                    }
                    else if (IsBorderLine(line))
                    {
                        Console.Write(_consoleDisplayParams.Border);
                    }
                }
                Console.WriteLine();
            }
        }

        private bool IsCommandLines(int line)
        {
            return line >= _consoleDisplayParams.Lines - _consoleDisplayParams.BottomPadding + 1;
        }

        private bool IsBorderLine(int line)
        {
            return line == 0 || 
                // is last line we should draw
                (line + _consoleDisplayParams.BottomPadding) == _consoleDisplayParams.Lines;
        }
    }
}