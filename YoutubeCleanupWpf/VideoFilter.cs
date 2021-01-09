namespace YoutubeCleanupWpf
{
    public class VideoFilter
    {
        public string Title { get; set; }
        public FilterType FilterType { get; set; }

        public VideoFilter Clone()
        {
            return (VideoFilter)MemberwiseClone();
        }
    }

    public enum FilterType
    {
        PlaylistTitle,
        Uncategorized,
        All
    }
}