using System.ComponentModel;
using System.Windows.Media.Imaging;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupWpf;

public class WpfVideoData : VideoData, INotifyPropertyChanged
{
#pragma warning disable 067
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
    public BitmapImage Thumbnail { get; set; }
}