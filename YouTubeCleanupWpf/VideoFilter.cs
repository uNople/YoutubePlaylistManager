using System.ComponentModel;

namespace YouTubeCleanupWpf;

public class VideoFilter : INotifyPropertyChanged
{
#pragma warning disable 067
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
    public string Title { get; set; }
    public string Id { get; set; }
    public FilterType FilterType { get; set; }
    public string OriginalTitle { get; set; }

    public VideoFilter Clone()
    {
        return (VideoFilter)MemberwiseClone();
    }
}