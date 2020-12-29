using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AutoMapper;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf;

namespace YoutubeCleanupWpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;
        private readonly IMapper _mapper;
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<VideoData> Videos { get; set; }
        public WpfVideoData SelectedVideo { get; set; }

        public MainWindowViewModel
            (
            [NotNull] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [NotNull] IMapper mapper
            )
        {
            _youTubeCleanupToolDbContext = youTubeCleanupToolDbContext;
            Videos = new ObservableCollection<VideoData>();
            _mapper = mapper;
        }

        public async Task LoadData()
        {
            var videos = await _youTubeCleanupToolDbContext.GetVideos();
            foreach (var video in videos.Where(x => x.ChannelTitle != null).Take(10))
            {
                WpfVideoData videoData = _mapper.Map<WpfVideoData>(video); 
                Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.Normal,
                    new Action(() =>
                    {
                        var image = CreateBitmapImageFromByteArray(videoData);
                        videoData.Thumbnail = image;
                    }));

                Videos.AddOnUI(videoData);
            }
        }

        private static BitmapImage CreateBitmapImageFromByteArray(WpfVideoData videoData)
        {
            var thumbnail = new BitmapImage();
            thumbnail.BeginInit();
            thumbnail.StreamSource = new MemoryStream(videoData.ThumbnailBytes);
            thumbnail.DecodePixelWidth = 200;
            thumbnail.EndInit();
            // Freeze so we can apparently move this between threads
            thumbnail.Freeze();
            return thumbnail;
        }
    }
    
    // TODO: better way?
    public static class CollectionExtensions
    {
        public static void AddOnUI<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }
    }
}
