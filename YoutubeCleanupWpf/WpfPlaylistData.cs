using System.ComponentModel;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class WpfPlaylistData : PlaylistData, INotifyPropertyChanged
    {
        #pragma warning disable 0067
        public event PropertyChangedEventHandler PropertyChanged;
        #pragma warning restore 0067
        public bool VideoInPlaylist { get; set; }
    }
}
