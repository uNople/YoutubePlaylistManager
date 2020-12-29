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
        public ObservableCollection<PlaylistData> Playlists { get; set; }
        public WpfVideoData SelectedVideo { get; set; }

        private PlaylistData _selectedPlaylistDataFromComboBox;
        public PlaylistData SelectedPlaylistFromComboBox
        {
            get => _selectedPlaylistDataFromComboBox;
            set
            {
                _selectedPlaylistDataFromComboBox = value;
                // Might lock the UI if run synchronously - but to be confirmed
                GetVideosForPlaylist(value).GetAwaiter().GetResult();
            }
        }

        private async Task GetVideosForPlaylist(PlaylistData playlistData)
        {
            Videos.ClearOnUi();
            var videoIds = new HashSet<string>(playlistData.PlaylistItems.Select(x => x.VideoId));
            var videos = (await _youTubeCleanupToolDbContext.GetVideos());
            var justVideosFromThisPlaylist = videos.Where(x => videoIds.Contains(x.Id));
            foreach (var video in justVideosFromThisPlaylist)
            {
                AddVideoToCollection(video);
            }
        }

        public MainWindowViewModel
            (
            [NotNull] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [NotNull] IMapper mapper
            )
        {
            _youTubeCleanupToolDbContext = youTubeCleanupToolDbContext;
            Videos = new ObservableCollection<VideoData>();
            Playlists = new ObservableCollection<PlaylistData>();
            _mapper = mapper;
        }

        public async Task LoadData()
        {
            await GetVideos(100);

            var playlists = await _youTubeCleanupToolDbContext.GetPlaylists();
            foreach (var playlist in playlists)
            {
                Playlists.AddOnUi(playlist);
            }
        }

        private async Task GetVideos(int limit)
        {
            var videos = await _youTubeCleanupToolDbContext.GetVideos();
            foreach (var video in videos.Take(limit))
            {
                AddVideoToCollection(video);
            }
        }

        private void AddVideoToCollection(VideoData video)
        {
            WpfVideoData videoData = _mapper.Map<WpfVideoData>(video);
            Application.Current.Dispatcher.Invoke(
                DispatcherPriority.Normal,
                new Action(() =>
                {
                    var image = CreateBitmapImageFromByteArray(videoData);
                    videoData.Thumbnail = image;
                }));

            Videos.AddOnUi(videoData);
        }

        private static BitmapImage CreateBitmapImageFromByteArray(WpfVideoData videoData)
        {
            if (videoData.ThumbnailBytes.Length == 0)
                return null;

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
        public static void AddOnUi<T>(this ICollection<T> collection, T item)
        {
            Action<T> addMethod = collection.Add;
            Application.Current.Dispatcher.BeginInvoke(addMethod, item);
        }

        public static void ClearOnUi<T>(this ICollection<T> collection)
        {
            Application.Current.Dispatcher.BeginInvoke(collection.Clear);
        }
    }
}
