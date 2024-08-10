using System;
using System.Collections.Generic;

namespace YouTubeCleanupTool.Domain;

public class DataSorter : IComparer<IData>
{
    public int Compare(IData x, IData y)
    {
            if (ReferenceEquals(x, y)) return 0;
            if (string.Equals(x?.Id, y?.Id, StringComparison.InvariantCultureIgnoreCase)) return 0;
            if (string.Equals(x?.Title, y?.Title, StringComparison.InvariantCultureIgnoreCase)) return 0;
            if (x == null) return 1;
            if (y == null) return -1;
            var titleCompare = string.Compare(x.Title, y.Title, StringComparison.InvariantCultureIgnoreCase);
            if (titleCompare == 0)
            {
                return string.Compare(x.Id, y.Id, StringComparison.InvariantCultureIgnoreCase);
            }

            return titleCompare;
        }
}