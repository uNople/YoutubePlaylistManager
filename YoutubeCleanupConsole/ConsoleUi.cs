using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeCleanupTool.Interfaces;
using YoutubeCleanupTool.Utils;

namespace YoutubeCleanupConsole
{
    public class ConsoleUi : IConsoleUi
    {
        private readonly ConsoleDisplayParams _consoleDisplayParams;
        private readonly IWhereTheRubberHitsTheRoad _whereTheRubberHitsTheRoad;
        private readonly ICredentialManagerWrapper _credentialManagerWrapper;

        public ConsoleUi(ConsoleDisplayParams consoleDisplayParams,
            IWhereTheRubberHitsTheRoad whereTheRubberHitsTheRoad,
            ICredentialManagerWrapper credentialManagerWrapper)
        {
            _consoleDisplayParams = consoleDisplayParams ?? throw new ArgumentNullException(nameof(consoleDisplayParams));
            _whereTheRubberHitsTheRoad = whereTheRubberHitsTheRoad ?? throw new ArgumentNullException(nameof(whereTheRubberHitsTheRoad));
            _credentialManagerWrapper = credentialManagerWrapper ?? throw new ArgumentNullException(nameof(credentialManagerWrapper));
        }

        public async Task Run()
        {
            var commands = new Dictionary<string, Func<Task>>(StringComparer.InvariantCultureIgnoreCase)
            {
                { "GetPlaylists", async () => await GetPlaylists() },
                { "UpdateApiKey", async () => await Task.Run(() => _credentialManagerWrapper.PromptForKey()) }
            };

            //PromptForKeyIfNotExists();

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
                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                    }
                }
            }
        }

        private async Task GetPlaylists()
        {
            Console.WriteLine("Playlist Details:");
            Console.WriteLine();
            (await _whereTheRubberHitsTheRoad.GetPlaylists())
                .ForEach(x => Console.WriteLine($"{x.Id} - {x.Snippet.Title}"));
        }

        // TODO: Move these Check Credential things into another class - but... maybe don't need to move it?
        // TODO: Write tests
        private void CheckClientJsonExists()
        {
            // TODO: prompt for new path for client.json
            // make sure it exists there
            // validate we can create a YoutubeClientwrapper at this point?
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