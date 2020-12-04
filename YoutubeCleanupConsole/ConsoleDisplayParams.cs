using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeCleanupConsole
{
    public class ConsoleDisplayParams
    {
        public int Columns { get; set; } = 120;
        public int Lines { get; set; } = 29;
        public char Filler { get; set; } = ' ';
        public char Border { get; set; } = '.';
        public int InsidePadding { get; set; } = 4;
        public int BottomPadding { get; set; } = 3;
    }
}
