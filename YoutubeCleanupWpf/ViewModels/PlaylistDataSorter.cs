using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf.ViewModels
{
    public class DataSorter : IComparer<IData>
    {
        public int Compare(IData x, IData y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (ReferenceEquals(null, y)) return 1;
            if (ReferenceEquals(null, x)) return -1;
            return string.Compare(x.Title, y.Title, StringComparison.Ordinal);
        }
    }
}
