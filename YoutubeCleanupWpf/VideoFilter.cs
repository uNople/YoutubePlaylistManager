namespace YoutubeCleanupWpf
{
    public class VideoFilter
    {
        public string Title { get; set; }
        public FilterType FilterType { get; set; }
    }

    public enum FilterType
    {
        PlaylistTitle,
        Uncategorized,
        All
    }
}