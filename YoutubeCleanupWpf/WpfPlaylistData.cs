using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class WpfPlaylistData : PlaylistData, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public bool VideoInPlaylist { get; set; }
    }
}
