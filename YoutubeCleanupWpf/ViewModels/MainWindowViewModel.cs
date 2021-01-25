using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AutoMapper;
using Google.Apis.YouTube.v3.Data;
using YouTubeApiWrapper.Interfaces;
using YouTubeCleanupTool.DataAccess;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf.Converters;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel
        (
            [NotNull] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            [NotNull] IMapper mapper,
            [NotNull] IGetAndCacheYouTubeData getAndCacheYouTubeData,
            [NotNull] IUpdateDataViewModel updateDataViewModel,
            [NotNull] IWindowService windowService
        )
        {
            _youTubeCleanupToolDbContextFactory = youTubeCleanupToolDbContextFactory;
            Videos = new ObservableCollection<VideoData>();
            Playlists = new ObservableCollection<WpfPlaylistData>();
            VideoFilter = new ObservableCollection<VideoFilter>();
            _mapper = mapper;
            _getAndCacheYouTubeData = getAndCacheYouTubeData;
            CheckedOrUncheckedVideoInPlaylistCommand = new RunMethodCommand<WpfPlaylistData>(async o => await UpdateVideoInPlaylist(o), ShowError);
            OpenPlaylistCommand = new RunMethodCommand<PlaylistData>(OpenPlaylist, ShowError);
            OpenChannelCommand = new RunMethodCommand<VideoData>(OpenChannel, ShowError);
            OpenVideoCommand = new RunMethodCommand<VideoData>(OpenVideo, ShowError);
            SearchCommand = new RunMethodWithoutParameterCommand(Search, ShowError);
            RefreshDataCommand = new RunMethodWithoutParameterCommand(UpdateData, ShowError);
            UpdateSettingsCommand = new RunMethodWithoutParameterCommand(UpdateSettings, ShowError);
            _searchTypeDelayDeferTimer = new DeferTimer(async () => await SearchForVideos(SearchText), ShowError);
            _selectedFilterDataFromComboBoxDeferTimer = new DeferTimer(async () => await GetVideosForPlaylist(SelectedFilterFromComboBox), ShowError);
            _updateDataViewModel = updateDataViewModel;
            _windowService = windowService;
            SpecialVideoFilters = new List<VideoFilter>()
            {
                new VideoFilter {Title = "All", FilterType = FilterType.All},
                new VideoFilter {Title = "Uncategorized", FilterType = FilterType.Uncategorized},
            };
        }

        private readonly DeferTimer _selectedFilterDataFromComboBoxDeferTimer;
        private readonly DeferTimer _searchTypeDelayDeferTimer;
        private readonly IYouTubeCleanupToolDbContextFactory _youTubeCleanupToolDbContextFactory;
        private readonly IMapper _mapper;
        private Dictionary<string, List<string>> _videosToPlaylistMap = new Dictionary<string, List<string>>();
        private readonly IGetAndCacheYouTubeData _getAndCacheYouTubeData;
        private VideoFilter _preservedFilter;
        private readonly IUpdateDataViewModel _updateDataViewModel;
        private WpfVideoData _selectedVideo;
        private readonly IWindowService _windowService;

        public event PropertyChangedEventHandler PropertyChanged;
        public ICommand OpenVideoCommand { get; set; }
        public ICommand OpenPlaylistCommand { get; set; }
        public ICommand OpenChannelCommand { get; set; }
        public ICommand SearchCommand { get; set; }
        public ICommand CheckedOrUncheckedVideoInPlaylistCommand { get; set; }
        public ICommand RefreshDataCommand { get; set; }
        public ICommand UpdateSettingsCommand { get; set; }
        public ObservableCollection<VideoData> Videos { get; set; }
        public ObservableCollection<WpfPlaylistData> Playlists { get; set; }
        public ObservableCollection<VideoFilter> VideoFilter { get; set; }
        public string SearchResultCount { get; set; }
        public bool SearchActive { get; set; }
        public bool UpdateHappening { get; set; }
        public List<VideoFilter> SpecialVideoFilters { get; }


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
        public VideoFilter SelectedFilterFromComboBox
        {
            get => _selectedFilterDataFromComboBox;
            set
            {
                _selectedFilterDataFromComboBox = value;
                // This is here so when we select the videos we run that async. the async part happens in the DeferTimer
                // Prior to this, we had it run synchronously, and it froze the UI whenever we selected another filter
                // with a lot of videos
                _selectedFilterDataFromComboBoxDeferTimer.DeferByMilliseconds(2);
            }
        }

        private string _searchText;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                if (SearchActive)
                    _searchTypeDelayDeferTimer.DeferByMilliseconds(200);
            }
        }

        private async Task UpdateData()
        {
            UpdateHappening = true;
            var cancellationTokenSource = new CancellationTokenSource();
            
            _windowService.ShowUpdateDataWindow();
            _updateDataViewModel.CancellationTokenSource = cancellationTokenSource;
            _updateDataViewModel.MainWindowViewModel = this;


            void Callback(IData data, InsertStatus status)
            {
                _updateDataViewModel.PrependText($"{data.GetType().Name} - {data.Title} - {status}");
            }

            _updateDataViewModel.PrependText("Updating playlists");
            await _getAndCacheYouTubeData.GetPlaylists(Callback);
            _updateDataViewModel.PrependText("Updating playlist items");
            await _getAndCacheYouTubeData.GetPlaylistItems(Callback);
            _updateDataViewModel.PrependText("Updating videos");
            await _getAndCacheYouTubeData.GetVideos(Callback, false, cancellationTokenSource.Token);
            _updateDataViewModel.PrependText("Completed! - You can close the window now.");

            await LoadData();
        }
        
        private async Task UpdateSettings()
        {
            _windowService.ShowSettingsWindow();
        }

        public async Task LoadData()
        {
            var playlists = (await _youTubeCleanupToolDbContextFactory.Create().GetPlaylists())?.OrderBy(x => x.Title).ToList() ?? new List<PlaylistData>();
            var playlistItems = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylistItems() ?? new List<PlaylistItemData>();
            _videosToPlaylistMap = playlistItems
                ?.Where(x => x.VideoId != null)
                .GroupBy(x => x.VideoId)
                .ToDictionary(x => x.Key, x => x.Select(y => y.PlaylistDataId).ToList());

            if (Playlists.Count == 0)
            {
                playlists.ForEach(x => Playlists.AddOnUi(_mapper.Map<WpfPlaylistData>(x)));
            }
            else
            {
                var comparer = new PlaylistDataSorter();
                
                var mappedPlaylists = _mapper.Map<List<WpfPlaylistData>>(playlists);
                foreach (var playlist in mappedPlaylists)
                {
                    var compareResult = Playlists.ToList().BinarySearch(playlist, comparer);
                    if (compareResult < 0)
                    {
                        // InsertOnUi?
                        Playlists.Insert(~compareResult, playlist);
                    }
                }

                var playlistsToRemove = new List<WpfPlaylistData>();
                foreach (var playlist in Playlists)
                {
                    if (!mappedPlaylists.Any(x => x.Id == playlist.Id))
                    {
                        playlistsToRemove.Add(playlist);
                    }
                }
                
                foreach (var removeThis in playlistsToRemove)
                {
                    Playlists.Remove(removeThis);
                }
            }

            if (VideoFilter.Count == 0)
            {
                SpecialVideoFilters.ForEach(x => VideoFilter.AddOnUi(x));
                foreach (var playlist in playlists.OrderBy(x => x.Title))
                {
                    VideoFilter.AddOnUi(new VideoFilter {Title = playlist.Title, FilterType = FilterType.PlaylistTitle});
                }
            }
            else
            {
                // insert new playlists in the right place
            }

            if (SelectedFilterFromComboBox == null)
            {
                await GetVideos(100);
            }
            else
            {
                // get videos for the selected filter/playlist, then add any new ones / delete any deleted ones
            }
        }
        
        private async Task OpenChannel(VideoData videoData) => await Task.Run(() => OpenLink($"https://www.youtube.com/channel/{videoData.ChannelId}"));
        private async Task OpenPlaylist(PlaylistData playlistData) => await Task.Run(() => OpenLink($"https://www.youtube.com/playlist?list={playlistData.Id}"));
        public static void ShowError(Exception ex) => MessageBox.Show(ex.ToString());
        private async Task OpenVideo(VideoData videoData) => await Task.Run(() => OpenLink($"https://www.youtube.com/watch?v={videoData.Id}"));

        public static void OpenLink(string url)
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
        
        private async Task SearchForVideos(string searchText)
        {
            Videos.ClearOnUi();

            if (string.IsNullOrEmpty(searchText))
            {
                SearchResultCount = "";
                return;
            }

            var videos = (await _youTubeCleanupToolDbContextFactory.Create().GetVideos());
            var videosFound = videos.Where(x => x.Title.ContainsCi(searchText)).OrderBy(x => x.Title).ToList();
            SearchResultCount = $"{videosFound.Count} videos found";
            foreach (var video in videosFound)
            {
                AddVideoToCollection(video);
            }
        }

        private async Task Search()
        {
            SearchActive = !SearchActive;
            if (!SearchActive)
            {
                SelectedFilterFromComboBox = _preservedFilter;
                if (SelectedFilterFromComboBox == null)
                {
                    await GetVideos(100);
                }
            }
            else
            {
                _preservedFilter = _selectedFilterDataFromComboBox?.Clone();
                await SearchForVideos(SearchText);
            }
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
                var matchingPlaylist = Playlists.First(x => x.Title == videoFilter.Title);
                var videos = (await _youTubeCleanupToolDbContextFactory.Create().GetVideos());
                foreach (var videoId in matchingPlaylist.PlaylistItems.OrderBy(x => x.Position).Select(x => x.VideoId))
                {
                    AddVideoToCollection(videos.FirstOrDefault(x => x.Id == videoId));
                }
            }
            else if (videoFilter.FilterType == FilterType.All)
            {
                var videos = (await _youTubeCleanupToolDbContextFactory.Create().GetVideos());
                foreach (var video in videos)
                {
                    AddVideoToCollection(video);
                }
            }
            else if (videoFilter.FilterType == FilterType.Uncategorized)
            {
                // TODO: Create some way of indicating a playlist is a "dumping ground" playlist - meaning videos only in that should be uncategorized
                var playlistsThatMeanUncategorized = new List<string> {"Liked videos", "!WatchLater"};
                var videos = (await _youTubeCleanupToolDbContextFactory.Create().GetUncategorizedVideos(playlistsThatMeanUncategorized));
                foreach (var video in videos)
                {
                    AddVideoToCollection(video);
                }
            }
        }
        
        private void SelectedVideoChanged(WpfVideoData video)
        {
            if (video == null)
            {
                foreach (var playlistItem in Playlists)
                {
                    if (playlistItem.VideoInPlaylist)
                    {
                        WpfExtensions.RunOnUiThread(() => playlistItem.VideoInPlaylist = false);
                    }
                }
                return;
            }
            
            if (_videosToPlaylistMap.TryGetValue(video.Id, out var playlistItems))
            {
                var playlistItemsHashSet = new HashSet<string>(playlistItems);
                foreach (var playlistItem in Playlists)
                {
                    if (playlistItemsHashSet.Contains(playlistItem.Id) && !playlistItem.VideoInPlaylist)
                    {
                        WpfExtensions.RunOnUiThread(() => playlistItem.VideoInPlaylist = true);
                    }
                    else if (playlistItem.VideoInPlaylist)
                    {
                        WpfExtensions.RunOnUiThread(() => playlistItem.VideoInPlaylist = false);
                    }
                }
            }
        }
        
        private async Task GetVideos(int limit)
        {
            var videos = await _youTubeCleanupToolDbContextFactory.Create().GetVideos();
            if (videos == null)
                return;
            foreach (var video in videos.Take(limit))
            {
                AddVideoToCollection(video);
            }
        }

        private void AddVideoToCollection(VideoData video)
        {
            WpfVideoData videoData = _mapper.Map<WpfVideoData>(video);
            new Action(() =>
            {
                var image = CreateBitmapImageFromByteArray(videoData);
                videoData.Thumbnail = image;
            }).RunOnUiThread();

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
}
