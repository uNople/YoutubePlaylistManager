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
using Newtonsoft.Json;
using YouTubeCleanup.Ui;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupWpf.ViewModels;

public class MainWindowViewModel : IMainWindowViewModel, INotifyPropertyChanged
{
    public MainWindowViewModel
    (
        [NotNull] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
        [NotNull] IMapper mapper,
        [NotNull] IGetAndCacheYouTubeData getAndCacheYouTubeData,
        [NotNull] IUpdateDataViewModel updateDataViewModel,
        [NotNull] IWindowService windowService,
        [NotNull] IErrorHandler errorHandler,
        [NotNull] IDoWorkOnUi doWorkOnUi,
        [NotNull] IDebugSettings debugSettings,
        [NotNull] ILogger logger
    )
    {
        _youTubeCleanupToolDbContextFactory = youTubeCleanupToolDbContextFactory;
        Videos = new ObservableCollection<WpfVideoData>();
        Playlists = new ObservableCollection<WpfPlaylistData>();
        VideoFilter = new ObservableCollection<VideoFilter>();
        _mapper = mapper;
        _getAndCacheYouTubeData = getAndCacheYouTubeData;
        CheckedOrUncheckedVideoInPlaylistCommand =
            new RunMethodCommand<WpfPlaylistData>(async o => await UpdateVideoInPlaylist(o), errorHandler.HandleError);
        OpenPlaylistCommand = new RunMethodCommand<PlaylistData>(OpenPlaylist, errorHandler.HandleError);
        OpenChannelCommand = new RunMethodCommand<VideoData>(OpenChannel, errorHandler.HandleError);
        OpenVideoCommand = new RunMethodCommand<VideoData>(OpenVideo, errorHandler.HandleError);
        SearchCommand = new RunMethodWithoutParameterCommand(Search, errorHandler.HandleError);
        RefreshDataCommand = new RunMethodWithoutParameterCommand(UpdateAllPlaylists, errorHandler.HandleError);
        UpdateSettingsCommand = new RunMethodWithoutParameterCommand(UpdateSettings, errorHandler.HandleError);
        RefreshSelectedPlaylistCommand =
            new RunMethodWithoutParameterCommand(UpdateSelectedPlaylist, errorHandler.HandleError);
        ShowLogsCommand = new RunMethodWithoutParameterCommand(ShowLogsWindow, errorHandler.HandleError);
        _searchTypeDelayDeferTimer =
            new DeferTimer(async () => await SearchForVideos(SearchText), errorHandler.HandleError);
        _selectedFilterDataFromComboBoxDeferTimer =
            new DeferTimer(async () => await GetVideosForPlaylist(SelectedFilterFromComboBox),
                errorHandler.HandleError);
        _selectedVideoChangedDeferTimer = new DeferTimer(async () => await SelectedVideoChanged(SelectedVideo),
            errorHandler.HandleError);
        _updateDataViewModel = updateDataViewModel;
        _windowService = windowService;
        _doWorkOnUi = doWorkOnUi;
        _debugSettings = debugSettings;
        _logger = logger;
        _debugSettings.ShowIdsChanged += DebugSettingsOnShowIdsChanged;
        SpecialVideoFilters = new List<VideoFilter>()
        {
            new() { Title = "All", FilterType = FilterType.All },
            new() { Title = "Uncategorized", FilterType = FilterType.Uncategorized },
        };
    }

    private void DebugSettingsOnShowIdsChanged(bool showIds)
    {
        foreach (var videoFilter in VideoFilter)
        {
            if (videoFilter.FilterType == FilterType.PlaylistTitle)
            {
                _doWorkOnUi.RunOnUiThread(() =>
                    videoFilter.Title = MakePlaylistTitle(videoFilter.OriginalTitle, videoFilter.Id, showIds));
            }
        }

        foreach (var playlist in Playlists)
        {
            playlist.DisplayTitle = MakePlaylistTitle(playlist.Title, playlist.Id, showIds);
        }
    }

    private readonly DeferTimer _selectedFilterDataFromComboBoxDeferTimer;
    private readonly DeferTimer _selectedVideoChangedDeferTimer;
    private readonly DeferTimer _searchTypeDelayDeferTimer;
    private readonly IYouTubeCleanupToolDbContextFactory _youTubeCleanupToolDbContextFactory;
    private readonly IMapper _mapper;
    private Dictionary<string, List<string>> _videosToPlaylistMap = new();
    private readonly IGetAndCacheYouTubeData _getAndCacheYouTubeData;
    private VideoFilter _preservedFilter;
    private readonly IUpdateDataViewModel _updateDataViewModel;
    private WpfVideoData _selectedVideo;
    private readonly IWindowService _windowService;
    private readonly IDoWorkOnUi _doWorkOnUi;
    private readonly IDebugSettings _debugSettings;
    private readonly ILogger _logger;

#pragma warning disable 067
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
    public ICommand OpenVideoCommand { get; set; }
    public ICommand OpenPlaylistCommand { get; set; }
    public ICommand OpenChannelCommand { get; set; }
    public ICommand SearchCommand { get; set; }
    public ICommand CheckedOrUncheckedVideoInPlaylistCommand { get; set; }
    public ICommand RefreshDataCommand { get; set; }
    public ICommand UpdateSettingsCommand { get; set; }
    public ICommand RefreshSelectedPlaylistCommand { get; set; }
    public ICommand ShowLogsCommand { get; set; }
    public ObservableCollection<WpfVideoData> Videos { get; set; }
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
            _selectedVideoChangedDeferTimer.DeferByMilliseconds(2);
        }
    }

    private VideoFilter _selectedFilterDataFromComboBox;

    public VideoFilter SelectedFilterFromComboBox
    {
        get => _selectedFilterDataFromComboBox;
        set
        {
            _selectedFilterDataFromComboBox = value;

            // TODO: need a better way of doing this, like post-load or something we enable this?
            if (ShouldSelectingFilterUpdateVideos)
            {
                // This is here so when we select the videos we run that async. the async part happens in the DeferTimer
                // Prior to this, we had it run synchronously, and it froze the UI whenever we selected another filter
                // with a lot of videos
                _selectedFilterDataFromComboBoxDeferTimer.DeferByMilliseconds(2);
            }
        }
    }

    public bool ShouldSelectingFilterUpdateVideos { get; set; } = true;

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

    private async Task UpdateSelectedPlaylist()
    {
        if (SelectedFilterFromComboBox.FilterType != FilterType.PlaylistTitle)
        {
            return;
        }

        var matchingPlaylist = Playlists.First(x => x.Id == SelectedFilterFromComboBox.Id);

        await DoRefreshFromYouTube(async (callback, cancellationToken) =>
            {
                _logger.Info("Updating playlist");
                await _getAndCacheYouTubeData.GetPlaylistItemsForPlaylist(callback, matchingPlaylist,
                    cancellationToken);
                _logger.Info("Updating videos");
                await _getAndCacheYouTubeData.GetVideos(callback, false, cancellationToken);
                _logger.Info("Completed! - You can close the window now.");
            }, $"Update playlist {matchingPlaylist.Title}");
    }

    private async Task UpdateAllPlaylists()
    {
        await DoRefreshFromYouTube(async (callback, cancellationToken) =>
        {
            // TODO: Work out and store an average of how long these things take
            // Then work out the number of steps per method based on how long it takes
            // Use that to set the max progress bar value
            /*
             * Implementation Idea for this:
             * - Store in the DB some times for each run of this
             * - Select out things like how many of each thing we have vs how long a run takes on average
             * - Exclude runs that did things like reauth, or runs that were cancelled
             */
            _logger.Info("Updating playlists");
            await _getAndCacheYouTubeData.GetPlaylists(callback, cancellationToken);
            _logger.Info("Updating playlist items");
            await _getAndCacheYouTubeData.GetPlaylistItems(callback, cancellationToken);
            _logger.Info("Updating videos");
            await _getAndCacheYouTubeData.GetVideos(callback, false, cancellationToken);
            _logger.Info("Completed! - You can close the window now.");
        }, "Update All Playlists from YouTube");
    }

    private async Task ShowLogsWindow()
    {
        await _windowService.ShowUpdateDataWindow("Logs");
    }

    private async Task DoRefreshFromYouTube(
        Func<Func<IData, InsertStatus, CancellationToken, Task>, CancellationToken, Task> refresh, string title)
    {
        UpdateHappening = true;
        var cancellationTokenSource = new CancellationTokenSource();
        var runGuid = Guid.NewGuid();

        try
        {
            await _updateDataViewModel.CreateNewActiveTask(runGuid, title, cancellationTokenSource);
            await _windowService.ShowUpdateDataWindow(title);
            await _updateDataViewModel.SetNewProgressMax(10000);

            async Task Callback(IData data, InsertStatus status, CancellationToken cancellationToken)
            {
                _logger.Info($"{data.GetType().Name} - {data.DisplayInfo()} - {status}");
                await _updateDataViewModel.IncrementProgress();
            }

            await Task.Run(async () =>
            {
                try
                {
                    await refresh(Callback, cancellationTokenSource.Token);
                }
                catch (Exception ex) when (ex is TaskCanceledException or OperationCanceledException)
                {
                    // If we cancel, it's no big deal   
                }

                await LoadData();
            }, cancellationTokenSource.Token);
        }
        finally
        {
            await _updateDataViewModel.ResetProgress();
            UpdateHappening = false;
            await _updateDataViewModel.SetActiveTaskComplete(runGuid, title);
        }
    }

    private async Task UpdateSettings()
    {
        await Task.Run(() => _windowService.ShowSettingsWindow());
    }

    private async Task DoNotRunFilterUpdate(Func<Task> action)
    {
        try
        {
            ShouldSelectingFilterUpdateVideos = false;
            await action();
        }
        finally
        {
            ShouldSelectingFilterUpdateVideos = true;
        }
    }

    public async Task LoadData() => await DoNotRunFilterUpdate(async () =>
    {
        var playlists =
            (await _youTubeCleanupToolDbContextFactory.Create().GetPlaylists())?.OrderBy(x => x, new DataSorter())
            .ToList() ?? new List<PlaylistData>();
        var playlistItems = await _youTubeCleanupToolDbContextFactory.Create().GetPlaylistItems() ??
                            new List<PlaylistItemData>();
        var playlistItemsByPlaylist = playlistItems.GroupBy(x => x.PlaylistDataId)
            // TODO: figure out why sometimes playlist items have a null playlist
            .Where(x => x.Key != null)
            .ToDictionary(x => x.Key, x => x.Select(y => y));
        _videosToPlaylistMap = playlistItems
            .Where(x => x.VideoId != null)
            .GroupBy(x => x.VideoId)
            .ToDictionary(x => x.Key, x => x.Select(y => y.PlaylistDataId).ToList());

        var comparer = new DataSorter();

        if (Playlists.Count == 0)
        {
            playlists.ForEach(x => _doWorkOnUi.AddOnUi(Playlists, MapWpfPlaylistData(x)));
        }
        else
        {
            var mappedPlaylists = MapWpfPlaylistData(playlists);
            foreach (var playlist in mappedPlaylists)
            {
                var compareResult = Playlists.ToList().BinarySearch(playlist, comparer);
                if (compareResult < 0)
                {
                    // InsertOnUi?
                    await _doWorkOnUi.RunOnUiThreadAsync(() => Playlists.Insert(~compareResult, playlist));
                }
            }

            var playlistsToRemove = new List<WpfPlaylistData>();
            foreach (var playlist in Playlists)
            {
                if (!mappedPlaylists.Any(x => x.Id == playlist.Id))
                {
                    playlistsToRemove.Add(playlist);
                }
                else
                {
                    _doWorkOnUi.ClearOnUi(playlist.PlaylistItems);
                    if (playlistItemsByPlaylist.ContainsKey(playlist.Id))
                    {
                        playlistItemsByPlaylist[playlist.Id]
                            .ForEach(x => _doWorkOnUi.AddOnUi(playlist.PlaylistItems, x));
                    }
                }
            }

            foreach (var removeThis in playlistsToRemove)
            {
                _doWorkOnUi.RemoveOnUi(Playlists, removeThis);
            }
        }

        if (VideoFilter.Count == 0)
        {
            SpecialVideoFilters.ForEach(x => _doWorkOnUi.AddOnUi(VideoFilter, x));
            foreach (var playlist in playlists.OrderBy(x => x.Title))
            {
                _doWorkOnUi.AddOnUi(VideoFilter,
                    new VideoFilter
                    {
                        OriginalTitle = playlist.Title,
                        Title = MakePlaylistTitle(playlist.Title, playlist.Id, _debugSettings.ShowIds),
                        FilterType = FilterType.PlaylistTitle, Id = playlist.Id
                    });
            }
        }
        else
        {
            // insert new playlists in the right place
        }

        if (SelectedFilterFromComboBox == null)
        {
            const int defaultVideoCount = 100;
            await GetVideos(defaultVideoCount);
            SearchResultCount = $"{defaultVideoCount} videos found";
        }
        else if (SelectedFilterFromComboBox.FilterType == FilterType.PlaylistTitle)
        {
            var matchingPlaylist = Playlists.First(x => x.Id == SelectedFilterFromComboBox.Id);
            _logger.Debug($"Dealing with playlist '{matchingPlaylist.DisplayInfo()}");

            var videoIds = new HashSet<string>(playlistItemsByPlaylist[matchingPlaylist.Id].Select(x => x.VideoId));
            _logger.Debug(
                $"{videoIds.Count} videos exist in playlist '{matchingPlaylist.DisplayInfo()}'. Ids: {string.Join(", ", videoIds)}");
            var videos = _mapper.Map<List<WpfVideoData>>(await _youTubeCleanupToolDbContextFactory.Create().GetVideos())
                .Where(x => videoIds.Contains(x.Id))
                .ToList();
            SearchResultCount = $"{videos.Count} videos found";
            _logger.Debug($"Videos from DB: {SerializeVideoCollection(videos)}");
            _logger.Debug($"Videos from UI: {SerializeVideoCollection(Videos.ToList())}");

            foreach (var video in videos)
            {
                var compareResult = Videos.ToList().BinarySearch(video, comparer);
                if (compareResult < 0)
                {
                    var image = await CreateBitmapImageFromByteArray(video);
                    video.Thumbnail = image;
                    await _doWorkOnUi.RunOnUiThreadAsync(() => Videos.Insert(~compareResult, video));
                    _logger.Debug(
                        $"Video {video.DisplayInfo()} wasn't found in the right order I guess, so we inserted it");
                }
                // TODO: handle rename of title in playlist item - Compare based on ID, not title. Then, we can check title, or just map what we got from YouTube over the top
                // Note for why:
                // It seems like YouTube reuses Ids in playlists for PlaylistItems
                // Due to this, we don't know if the title not being there means it's a brand new item, or it replaced something we have locally

                var videosById = videos.ToDictionary(x => x.Id, x => x);
                var videosToRemove = new List<WpfVideoData>();
                foreach (var videoData in Videos)
                {
                    if (!videosById.ContainsKey(videoData.Id))
                    {
                        videosToRemove.Add(videoData);
                    }
                }

                foreach (var removeThis in videosToRemove)
                {
                    _doWorkOnUi.RemoveOnUi(Videos, removeThis);
                    _logger.Debug($"Video {video.DisplayInfo()} got removed from the UI, it was no longer found");
                }
            }
        }
    });

    private List<WpfPlaylistData> MapWpfPlaylistData(List<PlaylistData> playlists)
    {
        return new List<WpfPlaylistData>(playlists.Select(MapWpfPlaylistData));
    }

    private WpfPlaylistData MapWpfPlaylistData(PlaylistData playlistData)
    {
        var playlist = _mapper.Map<WpfPlaylistData>(playlistData);
        playlist.DisplayTitle = MakePlaylistTitle(playlistData.Title, playlistData.Id, _debugSettings.ShowIds);
        return playlist;
    }

    private static string MakePlaylistTitle(string title, string id, bool showIds)
    {
        return $"{title}{(showIds ? $" ({id})" : "")}";
    }

    private async Task OpenChannel(VideoData videoData) =>
        await Task.Run(() => OpenLink($"https://www.youtube.com/channel/{videoData.ChannelId}"));

    private async Task OpenPlaylist(PlaylistData playlistData) => await Task.Run(() => OpenLink(playlistData.Url));
    private async Task OpenVideo(VideoData videoData) => await Task.Run(() => OpenLink(videoData.Url));

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
        _doWorkOnUi.ClearOnUi(Videos);

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
            await AddVideoToCollection(video);
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
                var playlistItem =
                    await _getAndCacheYouTubeData.AddVideoToPlaylist(wpfPlaylistData.Id, _selectedVideo.Id);
                if (_videosToPlaylistMap.TryGetValue(_selectedVideo.Id, out var playlists))
                {
                    if (!playlists.Contains(playlistItem.PlaylistDataId))
                    {
                        playlists.Add(playlistItem.PlaylistDataId);
                        _logger.Debug(
                            $"Adding video {_selectedVideo.DisplayInfo()} to playlist {wpfPlaylistData.DisplayInfo()}");
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
                        _logger.Debug(
                            $"Removing video {_selectedVideo.DisplayInfo()} from playlist {wpfPlaylistData.DisplayInfo()}");
                    }
                }
            }
        }
    }

    private async Task GetVideosForPlaylist(VideoFilter videoFilter)
    {
        _doWorkOnUi.ClearOnUi(Videos);
        SearchResultCount = "...";
        if (videoFilter.FilterType == FilterType.PlaylistTitle)
        {
            // Make this a method or something. I think I use this pattern elsewhere
            var videoIds =
                new HashSet<string>(Playlists.First(x => x.Id == videoFilter.Id).PlaylistItems.Select(x => x.VideoId));
            var videos = (await _youTubeCleanupToolDbContextFactory.Create().GetVideos())
                .Where(x => videoIds.Contains(x.Id))
                .OrderBy(x => x, new DataSorter())
                .ToList();
            SearchResultCount = $"{videos.Count} videos found";
            _logger.Debug(
                $"Videos after selecting a playlist: {SerializeVideoCollection(_mapper.Map<List<WpfVideoData>>(videos))}");
            foreach (var video in videos)
            {
                await AddVideoToCollection(video);
            }

            _logger.Debug($"Videos after selecting a playlist: {SerializeVideoCollection(Videos.ToList())}");
        }
        else if (videoFilter.FilterType == FilterType.All)
        {
            var videos = (await _youTubeCleanupToolDbContextFactory.Create().GetVideos());
            SearchResultCount = $"{videos.Count} videos found";
            foreach (var video in videos)
            {
                await AddVideoToCollection(video);
            }
        }
        else if (videoFilter.FilterType == FilterType.Uncategorized)
        {
            // TODO: Create some way of indicating a playlist is a "dumping ground" playlist - meaning videos only in that should be uncategorized
            // NOTE: unfortunately the watch later playlist isn't available in the YouTube data API
            var playlistsThatMeanUncategorized = new List<string> { "Liked videos", "!WatchLater" };
            var videos = (await _youTubeCleanupToolDbContextFactory.Create()
                .GetUncategorizedVideos(playlistsThatMeanUncategorized));
            SearchResultCount = $"{videos.Count} videos found";
            foreach (var video in videos)
            {
                await AddVideoToCollection(video);
            }
        }
    }

    private async Task SelectedVideoChanged(WpfVideoData video)
    {
        if (video == null)
        {
            foreach (var playlistItem in Playlists)
            {
                if (playlistItem.VideoInPlaylist)
                {
                    await _doWorkOnUi.RunOnUiThreadAsync(() => playlistItem.VideoInPlaylist = false);
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
                    await _doWorkOnUi.RunOnUiThreadAsync(() => playlistItem.VideoInPlaylist = true);
                }
                else if (playlistItem.VideoInPlaylist)
                {
                    await _doWorkOnUi.RunOnUiThreadAsync(() => playlistItem.VideoInPlaylist = false);
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
            await AddVideoToCollection(video);
        }
    }

    private async Task AddVideoToCollection(VideoData video)
    {
        WpfVideoData videoData = _mapper.Map<WpfVideoData>(video);
        var image = await CreateBitmapImageFromByteArray(videoData);
        videoData.Thumbnail = image;
        _doWorkOnUi.AddOnUi(Videos, videoData);
    }

    private async Task<BitmapImage> CreateBitmapImageFromByteArray(WpfVideoData videoData)
    {
        if (videoData.ThumbnailBytes.Length == 0)
            return null;

        try
        {
            return await Task.Run(() =>
            {
                var thumbnail = new BitmapImage();
                thumbnail.BeginInit();
                thumbnail.StreamSource = new MemoryStream(videoData.ThumbnailBytes);
                thumbnail.DecodePixelWidth = 200;
                thumbnail.EndInit();
                // Freeze so we can move this between threads (eg, create on background thread, use on UI thread)
                thumbnail.Freeze();
                return thumbnail;
            });
        }
        catch (Exception ex)
        {
            _logger.Error($"Error creating thumbnail image: {ex}");
            return null;
        }
    }

    private string SerializeVideoCollection(List<WpfVideoData> videos)
    {
        return JsonConvert.SerializeObject(videos.Select(x => new { x.Title, x.Id }), Formatting.Indented);
    }

    public void CopySelectedVideoLinkToClipboard()
    {
        if (SelectedVideo == null)
            return;

        Clipboard.SetText(SelectedVideo.Url);
    }
}