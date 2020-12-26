using AutoMapper;
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
using YoutubeCleanupTool.Domain;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupConsole
{
    public class ConsoleUi : IConsoleUi
    {
        private readonly ConsoleDisplayParams _consoleDisplayParams;
        private readonly IYouTubeApi _youTubeApi;
        private readonly ICredentialManagerWrapper _credentialManagerWrapper;
        private readonly YoutubeServiceCreatorOptions _youtubeServiceCreatorOptions;
        private readonly IGetAndCacheYouTubeData _getAndCacheYouTubeData;
        private readonly IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;

        public ConsoleUi(
            [NotNull] ConsoleDisplayParams consoleDisplayParams,
            [NotNull] IYouTubeApi youTubeApi,
            [NotNull] ICredentialManagerWrapper credentialManagerWrapper,
            [NotNull] YoutubeServiceCreatorOptions youtubeServiceCreatorOptions,
            [NotNull] IGetAndCacheYouTubeData getAndCacheYouTubeData,
            [NotNull] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext)
        {
            _consoleDisplayParams = consoleDisplayParams;
            _youTubeApi = youTubeApi;
            _credentialManagerWrapper = credentialManagerWrapper;
            _youtubeServiceCreatorOptions = youtubeServiceCreatorOptions;
            _getAndCacheYouTubeData = getAndCacheYouTubeData;
            _youTubeCleanupToolDbContext = youTubeCleanupToolDbContext;
        }

        public async Task Run()
        {
            // This is so we can display unicode text
            // NOTE: If unicode text isn't displaying in the console, use MS Gothic font. It displays the most that I've found so far
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            void logCallback(IData data, InsertStatus status) => Console.WriteLine($"{data.Id} - {data.Title} was {status}");

            var commands = new Dictionary<string, Func<string, Task>>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "UpdateApiKey", async (s) => await Task.Run(() => _credentialManagerWrapper.PromptForKey()) },
                { "GetPlaylists", async (s) => await _getAndCacheYouTubeData.GetPlaylists(logCallback) },
                { "GetPlaylistItems", async (s) => await _getAndCacheYouTubeData.GetPlaylistItems(logCallback) },
                { "GetVideos", async (s) => await _getAndCacheYouTubeData.GetVideos(logCallback, false) },
                { "GetAllVideos", async (s) => await _getAndCacheYouTubeData.GetVideos(logCallback, true) },
                { "GetUnicodeVideoTitles", async (s) => await _getAndCacheYouTubeData.GetUnicodeVideoTitles((string title) => Console.WriteLine(title)) },
                { "Search", Search },
            };

            PromptForKeyIfNotExists();

            CheckClientJsonExists();

            while (true)
            {

                Draw(commands.Keys.ToList());

                var command = Console.ReadLine();

                if (commands.TryGetValue(command.Split(' ')[0], out var func))
                {
                    try
                    {
                        await func(command.Split(' ').Skip(1).FirstOrDefault());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error running command {command}. Error: {ex}");
                        PromptToContinue();
                    }
                }
            }
        }

        private async Task Search(string searchTerm)
        {
            Console.WriteLine($"Searching for term {searchTerm}");
            var searchResults = await _youTubeCleanupToolDbContext.FindAll(searchTerm);
            searchResults.ForEach(x => Console.WriteLine($"{x.GetType().Name} - {x.Title}"));
        }

        private static void PromptToContinue()
        {
            Console.WriteLine("Press ENTER to continue");
            Console.ReadLine();
        }

        // TODO: Move these Check Credential things into another class - but... maybe don't need to move it?
        // TODO: Write tests
        private void CheckClientJsonExists()
        {
            // TODO: Change this to a wrapper or something, so we can set this to exist and things will be nice with tests
            while (!File.Exists(_youtubeServiceCreatorOptions.ClientSecretPath))
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