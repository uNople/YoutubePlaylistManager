using System;
using System.Collections.Generic;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf.ViewModels
{
    public class DataSorter : IComparer<IData>
    {
        public int Compare(IData x, IData y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (string.Equals(x?.Id, y?.Id)) return 0;
            if (string.Equals(x?.Title, y?.Title)) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            var titleCompare = string.Compare(x.Title, y.Title, StringComparison.Ordinal);
            if (titleCompare == 0)
            {
                return string.Compare(x.Id, y.Id, StringComparison.Ordinal);
            }

            return titleCompare;
        }
    }
}
