using System.ComponentModel;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class WpfPlaylistData : PlaylistData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool VideoInPlaylist { get; set; }
    }
}
