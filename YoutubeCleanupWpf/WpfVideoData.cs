using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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
