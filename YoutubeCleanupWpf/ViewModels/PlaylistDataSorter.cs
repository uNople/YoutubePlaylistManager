using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeCleanupWpf.ViewModels
{
    public class PlaylistDataSorter : IComparer<WpfPlaylistData>
    {
        public int Compare(WpfPlaylistData x, WpfPlaylistData y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return string.Compare(x.Title, y.Title, StringComparison.Ordinal);
        }
    }
}
