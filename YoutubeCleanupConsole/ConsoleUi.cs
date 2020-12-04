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
    // TODO: Move basically everything out of here - it's just here temporarily... """"temporarily""""
    public class ConsoleUi : IConsoleUi
    {
        private readonly IYouTubeServiceWrapper _youTubeServiceWrapper;
        private readonly IPersister _persister;
        private readonly ConsoleDisplayParams _consoleDisplayParams;
        private readonly IWhereTheRubberHitsTheRoad _whereTheRubberHitsTheRoad;

        public ConsoleUi(IYouTubeServiceWrapper youTubeServiceWrapper, IPersister persister, ConsoleDisplayParams consoleDisplayParams,
            IWhereTheRubberHitsTheRoad whereTheRubberHitsTheRoad)
        {
            _youTubeServiceWrapper = youTubeServiceWrapper ?? throw new ArgumentNullException(nameof(youTubeServiceWrapper));
            _persister = persister ?? throw new ArgumentNullException(nameof(persister));
            _consoleDisplayParams = consoleDisplayParams ?? throw new ArgumentNullException(nameof(consoleDisplayParams));
            _whereTheRubberHitsTheRoad = whereTheRubberHitsTheRoad ?? throw new ArgumentNullException(nameof(whereTheRubberHitsTheRoad));
        }

        public async Task Run()
        {
            while (true)
            {
                Draw();

                

                var command = Console.ReadLine();

                if (command == "GetPlaylists")
                {
                    try
                    {
                        Console.WriteLine("Playlist Details:");
                        Console.WriteLine();
                        (await _whereTheRubberHitsTheRoad.GetPlaylists())
                            .ForEach(x => Console.WriteLine($"{x.Id} - {x.Snippet.Title}"));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex}");
                    }
                    Console.WriteLine("Press enter to go again");
                    Console.ReadLine();
                }
            }
        }

        private void Draw()
        {
            for (int line = 0; line < _consoleDisplayParams.Lines; line++)
            {
                for (int column = 0; column < _consoleDisplayParams.Columns; column++)
                {
                    if (line >= _consoleDisplayParams.Lines - _consoleDisplayParams.BottomPadding + 1)
                    {

                    }
                    else if (line == 0 || column == 0 || column == _consoleDisplayParams.Columns - 1 || 
                        // is last line we should draw
                        (line + _consoleDisplayParams.BottomPadding) == _consoleDisplayParams.Lines)
                    {
                        Console.Write(_consoleDisplayParams.Border);
                    }
                    else
                    {
                        Console.Write(_consoleDisplayParams.Filler);
                    }
                }
                Console.WriteLine();
            }
        }
    }
}