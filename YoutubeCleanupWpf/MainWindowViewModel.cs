using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using AutoMapper;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf;

namespace YoutubeCleanupWpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel
        (
            [NotNull] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [NotNull] IMapper mapper,
            [NotNull] IGetAndCacheYouTubeData getAndCacheYouTubeData
        )
        {
            _youTubeCleanupToolDbContext = youTubeCleanupToolDbContext;
            Videos = new ObservableCollection<VideoData>();
            AllPlaylists = new List<PlaylistData>();
            Playlists = new ObservableCollection<WpfPlaylistData>();
            VideoFilter = new ObservableCollection<VideoFilter>();
            _mapper = mapper;
            _getAndCacheYouTubeData = getAndCacheYouTubeData;
            CheckedOrUncheckedVideoInPlaylistCommand = new RunMethodCommand<WpfPlaylistData>(async o => await UpdateVideoInPlaylist(o), ShowError);
            OpenPlaylistCommand = new RunMethodCommand<PlaylistData>(OpenPlaylist, ShowError);
            OpenChannelCommand = new RunMethodCommand<VideoData>(OpenChannel, ShowError);
            OpenVideoCommand = new RunMethodCommand<VideoData>(OpenVideo, ShowError);
        }

        private readonly IYouTubeCleanupToolDbContext _youTubeCleanupToolDbContext;
        private readonly IMapper _mapper;
        public ICommand OpenVideoCommand { get; set; }
        public ICommand OpenPlaylistCommand { get; set; }
        public ICommand OpenChannelCommand { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<VideoData> Videos { get; set; }
        public ObservableCollection<WpfPlaylistData> Playlists { get; set; }
        private List<PlaylistData> AllPlaylists { get; set; }
        private WpfVideoData _selectedVideo;
        public ObservableCollection<VideoFilter> VideoFilter { get; set; }
        public ICommand CheckedOrUncheckedVideoInPlaylistCommand { get; set; }

        public WpfVideoData SelectedVideo
        {
            get => _selectedVideo;
            set
            {
                _selectedVideo = value;
                SelectedVideoChanged(value);
            }
        }

        private VideoFilter _selectedFilterDataFromComboBox;
        private Dictionary<string, List<string>> _videosToPlaylistMap = new Dictionary<string, List<string>>();
        private readonly IGetAndCacheYouTubeData _getAndCacheYouTubeData;
        
        public VideoFilter SelectedFilterFromComboBox
        {
            get => _selectedFilterDataFromComboBox;
            set
            {
                _selectedFilterDataFromComboBox = value;
                // Might lock the UI if run synchronously - but to be confirmed
                GetVideosForPlaylist(value).GetAwaiter().GetResult();
            }
        }
        
        private async Task OpenChannel(VideoData videoData)
        {
            OpenLink($"https://www.youtube.com/channel/{videoData.ChannelId}");
        }

        private async Task OpenPlaylist(PlaylistData playlistData)
        {
            OpenLink($"https://www.youtube.com/playlist?list={playlistData.Id}");
        }

        private static void OpenLink(string url)
        {
            // Why aren't we just using process.start? This is why: https://github.com/dotnet/runtime/issues/17938
            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                }
            };
            proc.Start();
        }

        private static void ShowError(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }

        private async Task OpenVideo(VideoData videoData)
        {
            OpenLink($"https://www.youtube.com/watch?v={videoData.Id}");
        }

        private async Task UpdateVideoInPlaylist(WpfPlaylistData wpfPlaylistData)
        {
            if (_selectedVideo != null)
            {
                // The playlist has just been ticked, so we want to add the video into this playlist
                if (wpfPlaylistData.VideoInPlaylist)
                {
                    var playlistItem = await _getAndCacheYouTubeData.AddVideoToPlaylist(wpfPlaylistData.Id, _selectedVideo.Id);
                    if (_videosToPlaylistMap.TryGetValue(_selectedVideo.Id, out var playlists))
                    {
                        if (!playlists.Contains(playlistItem.PlaylistDataId))
                        {
                            playlists.Add(playlistItem.PlaylistDataId);
                        }
                    }
                }
                else
                {
                    // If we just unticked a playlist, we want to remove the video from it
                    await _getAndCacheYouTubeData.RemoveVideoFromPlaylist(wpfPlaylistData.Id, _selectedVideo.Id);
                    if (_videosToPlaylistMap.TryGetValue(_selectedVideo.Id, out var playlists))
                    {
                        if (playlists.Contains(wpfPlaylistData.Id))
                        {
                            playlists.Remove(wpfPlaylistData.Id);
                        }
                    }
                }
            }
        }

        private async Task GetVideosForPlaylist(VideoFilter videoFilter)
        {
            Videos.ClearOnUi();
            if (videoFilter.FilterType == FilterType.PlaylistTitle)
            {
                var matchingPlaylist = AllPlaylists.First(x => x.Title == videoFilter.Title);
                var videos = (await _youTubeCleanupToolDbContext.GetVideos());
                foreach (var videoId in matchingPlaylist.PlaylistItems.OrderBy(x => x.Position).Select(x => x.VideoId))
                {
                    AddVideoToCollection(videos.FirstOrDefault(x => x.Id == videoId));
                }
            }
            else if (videoFilter.FilterType == FilterType.All)
            {
                var videos = (await _youTubeCleanupToolDbContext.GetVideos());
                foreach (var video in videos)
                {
                    AddVideoToCollection(video);
                }
            }
            else if (videoFilter.FilterType == FilterType.Uncategorized)
            {
                // TODO: get liked videos
                // check if they have more than one playlist assigned
                // If so, display them
            }
        }
        
        private void SelectedVideoChanged(WpfVideoData video)
        {
            if (video == null)
            {
                Playlists.ClearOnUi();
                return;
            }

            // Read out the playlists this video is in
            // Then tick/untick on the playlists object
            // And update the source the playlists are bound to

            var playlistData = new List<WpfPlaylistData>();
            var allPlaylists = new List<PlaylistData>(AllPlaylists);

            if (_videosToPlaylistMap.TryGetValue(video.Id, out var playlistItems))
            {
                foreach (var playlistItem in playlistItems)
                {
                    var matchingPlaylist = _mapper.Map<WpfPlaylistData>(allPlaylists.First(x => x.Id == playlistItem));
                    matchingPlaylist.VideoInPlaylist = true;
                    playlistData.Add(matchingPlaylist);
                }
            }

            playlistData.AddRange(_mapper.Map<List<WpfPlaylistData>>(allPlaylists.Where(x => !playlistData.Any(y => y.Id == x.Id))).OrderBy(x => x.Title));

            Playlists.ClearOnUi();
            foreach (var playlist in playlistData)
            {
                Playlists.AddOnUi(playlist);
            }
        }

        public async Task LoadData()
        {
            try
            {
                await GetVideos(100);

                var playlists = await _youTubeCleanupToolDbContext.GetPlaylists();
                var playlistItems = await _youTubeCleanupToolDbContext.GetPlaylistItems();
                _videosToPlaylistMap = playlistItems
                    .Where(x => x.VideoId != null)
                    .GroupBy(x => x.VideoId)
                    .ToDictionary(x => x.Key, x => x.Select(y => y.PlaylistDataId).ToList());
                    

                AllPlaylists.AddRange(playlists);

                VideoFilter.AddOnUi(new VideoFilter {Title = "All", FilterType = FilterType.All});
                VideoFilter.AddOnUi(new VideoFilter {Title = "Uncategorized", FilterType = FilterType.Uncategorized});
                foreach (var playlist in playlists.OrderBy(x => x.Title))
                {
                    VideoFilter.AddOnUi(new VideoFilter {Title = playlist.Title, FilterType = FilterType.PlaylistTitle});
                }
            }
            catch (Exception ex)
            {

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
