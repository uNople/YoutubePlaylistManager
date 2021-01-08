using System.ComponentModel;
using System.Windows.Media.Imaging;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class WpfVideoData : VideoData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public BitmapImage Thumbnail { get; set; }
    }
}
