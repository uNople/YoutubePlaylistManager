using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YouTubeApiWrapper.Interfaces;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupConsole
{
    public class ConsoleUi : IConsoleUi
    {
        private readonly ConsoleDisplayParams _consoleDisplayParams;
        private readonly ICredentialManagerWrapper _credentialManagerWrapper;
        private readonly YouTubeServiceCreatorOptions _youTubeServiceCreatorOptions;
        private readonly IGetAndCacheYouTubeData _getAndCacheYouTubeData;
        private readonly IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;
        private CancellationTokenSource _cancellationTokenSource;

        public ConsoleUi(
            [NotNull] ConsoleDisplayParams consoleDisplayParams,
            [NotNull] ICredentialManagerWrapper credentialManagerWrapper,
            [NotNull] YouTubeServiceCreatorOptions youTubeServiceCreatorOptions,
            [NotNull] IGetAndCacheYouTubeData getAndCacheYouTubeData,
            [NotNull] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext)
        {
            _consoleDisplayParams = consoleDisplayParams;
            _credentialManagerWrapper = credentialManagerWrapper;
            _youTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
            _getAndCacheYouTubeData = getAndCacheYouTubeData;
            _youTubeCleanupToolDbContext = youTubeCleanupToolDbContext;
        }

        public async Task Run()
        {
            // This is so we can display unicode text
            // NOTE: If unicode text isn't displaying in the console, use MS Gothic font. It displays the most that I've found so far
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            void logCallback(IData data, InsertStatus status) => Console.WriteLine($"{data.Id} - {data.Title} was {status}");

            var commands = new Dictionary<string, Func<string, CancellationToken, Task>>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "UpdateApiKey", async (_, c) => await Task.Run(() => _credentialManagerWrapper.PromptForKey(), c) },
                { "GetPlaylists", async (_, _) => await _getAndCacheYouTubeData.GetPlaylists(logCallback) },
                { "GetPlaylistItems", async (_, _) => await _getAndCacheYouTubeData.GetPlaylistItems(logCallback) },
                { "GetVideos", async (_, c) => await GetVideos(logCallback, false, c) },
                { "GetAllVideos", async (_, c) => await GetVideos(logCallback, true, c) },
                { "GetUnicodeVideoTitles", async (_, _) => await _getAndCacheYouTubeData.GetUnicodeVideoTitles(Console.WriteLine) },
                { "Search", async (s, c) => await Search(s, c) },
            };

            PromptForKeyIfNotExists();

            CheckClientJsonExists();

            while (true)
            {
                Draw(commands.Keys.ToList());

                var command = Console.ReadLine();

                _cancellationTokenSource = new CancellationTokenSource();

                if (command != null && commands.TryGetValue(command.Split(' ')[0], out var func))
                {
                    try
                    {
                        await func(command.Split(' ').Skip(1).FirstOrDefault(), _cancellationTokenSource.Token);
                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error running command {command}. Error: {ex}");
                        PromptToContinue();
                    }
                }
            }
        }

        private async Task GetVideos(Action<IData, InsertStatus> logCallback, bool getAllVideos, CancellationToken c)
        {
            Console.CancelKeyPress -= Console_CancelKeyPress;
            Console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                await _getAndCacheYouTubeData.GetVideos(logCallback, getAllVideos, c);
            }
            finally
            {
                Console.CancelKeyPress -= Console_CancelKeyPress;
            }
        }

        private void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            _cancellationTokenSource.Cancel();
            e.Cancel = true;
        }

        private async Task Search(string searchTerm, CancellationToken cancellationToken)
        {
            Console.CancelKeyPress -= Console_CancelKeyPress;
            Console.CancelKeyPress += Console_CancelKeyPress;

            try
            {
                Console.WriteLine($"Searching for term {searchTerm}");
                var searchResults = await _youTubeCleanupToolDbContext.FindAll(searchTerm);
                var originalBackgroundColor = Console.BackgroundColor;
                var originalForegroundColor = Console.ForegroundColor;
                var searchBackgroundColor = ConsoleColor.DarkYellow;
                var searchForegroundColor = ConsoleColor.Black;
                var regex = new Regex(searchTerm, RegexOptions.IgnoreCase | RegexOptions.Compiled);

                foreach (var searchResult in searchResults)
                {
                    Console.Write($"{searchResult.GetType().Name} - ");

                    var regexMatch = regex.Matches(searchResult.Title);

                    for (int i = 0; i < searchResult.Title.Length; i++)
                    {
                        bool matchFound = false;
                        foreach (Match match in regexMatch)
                        {
                            var startIndex = match.Index;
                            var length = match.Value.Length;
                            if (i >= startIndex && i <= startIndex + length - 1)
                            {
                                SetColor(searchBackgroundColor, searchForegroundColor);
                                matchFound = true;
                                break;
                            }
                        }

                        if (!matchFound)
                        {
                            SetColor(originalBackgroundColor, originalForegroundColor);
                        }

                        Console.Write(searchResult.Title[i]);
                    }
                    SetColor(originalBackgroundColor, originalForegroundColor);
                    Console.WriteLine();
                    if (cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine("Aborting - cancellation requested");
                        return;
                    }
                }
            }
            finally
            {
                Console.CancelKeyPress -= Console_CancelKeyPress;
            }
        }

        private static void SetColor(ConsoleColor background, ConsoleColor foreground)
        {
            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;
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
            while (!File.Exists(_youTubeServiceCreatorOptions.ClientSecretPath))
            {
                Console.WriteLine("Please download your client secret from google. URL should be: https://console.developers.google.com/?pli=1");
                Console.WriteLine($"Place client.json in {_youTubeServiceCreatorOptions.ClientSecretPath}");
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

            for (var line = 0; line < _consoleDisplayParams.Lines; line++)
            {
                if (!IsCommandLines(line) && !IsBorderLine(line))
                {
                    if (commandIndex < commands.Count)
                    {
                        Console.Write($" >   {commands[commandIndex++]}");
                    }
                }
                for (var column = 0; column < _consoleDisplayParams.Columns; column++)
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