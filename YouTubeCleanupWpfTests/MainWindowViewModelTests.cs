using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.UnitTests
{
    public class MainWindowViewModelTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public MainWindowViewModelTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory, AutoNSubstituteData]
        public async Task When_nothing_exists_Then_playlists_and_filters_and_videos_are_empty([NoAutoProperties]MainWindowViewModel mainWindowViewModel)
        {
            await mainWindowViewModel.LoadData();
            mainWindowViewModel.Playlists.Should().BeEmpty();
            mainWindowViewModel.Videos.Should().BeEmpty();
            mainWindowViewModel.VideoFilter.Should().BeEquivalentTo(mainWindowViewModel.SpecialVideoFilters);
        }

        [Theory, AutoNSubstituteData]
        public async Task When_new_playlist_created_on_YouTube_and_refresh_happens_Then_playlist_inserted_into_ui_in_the_correct_location(
            [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            [NoAutoProperties]MainWindowViewModel mainWindowViewModel
            )
        {
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);
            var playlistData = new List<PlaylistData>()
            {
                new() { Id = "2", Title = "c"},
                new() { Id = "1", Title = "a"},
            };

            var playlistDataTwo = new List<PlaylistData>()
            {
                new() {Id = "2", Title = "c"},
                new() {Id = "1", Title = "a"},
                new() {Id = "3", Title = "b"},
            };
            youTubeCleanupToolDbContext.GetPlaylists().Returns(playlistData, playlistDataTwo);

            await mainWindowViewModel.LoadData();

            mainWindowViewModel.Playlists.Should().BeEquivalentTo(new List<WpfPlaylistData>
            {
                new() { Id = "1", Title = "a"},
                new() { Id = "2", Title = "c"}
            }, WithDefaultListCompare());
            
            await mainWindowViewModel.LoadData();
            
            mainWindowViewModel.Playlists.Should().BeEquivalentTo(new List<WpfPlaylistData>
            {
                new() { Id = "1", Title = "a"},
                new() { Id = "3", Title = "b"},
                new() { Id = "2", Title = "c"}
            }, WithDefaultListCompare());
        }
        
        [Theory, AutoNSubstituteData]
        public async Task When_playlist_deleted_from_YouTube_and_refresh_happens_Then_playlist_removed_from_ui(
            [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            [NoAutoProperties]MainWindowViewModel mainWindowViewModel
        )
        {
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);
            var playlistData = new List<PlaylistData>()
            {
                new() { Id = "2", Title = "c"},
                new() { Id = "1", Title = "a"},
            };
            var playlistDataTwo = new List<PlaylistData>()
            {
                new() { Id = "2", Title = "c"},
            };
            
            youTubeCleanupToolDbContext.GetPlaylists().Returns(playlistData, playlistDataTwo);

            await mainWindowViewModel.LoadData();
            mainWindowViewModel.Playlists.Should().BeEquivalentTo(new List<WpfPlaylistData>
            {
                new() { Id = "1", Title = "a"},
                new() { Id = "2", Title = "c"}
            });

            await mainWindowViewModel.LoadData();
            mainWindowViewModel.Playlists.Should().BeEquivalentTo(new List<WpfPlaylistData>
            {
                new() { Id = "2", Title = "c"}
            }, WithDefaultListCompare());
        }

        [Theory, AutoNSubstituteData]
        public async Task When_video_added_to_playlist_on_YouTube_and_refresh_happens_Then_video_inserted_into_ui(
            [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            WpfPlaylistData playlistData,
            VideoData videoData,
            IMapper mapper,
            [NoAutoProperties]MainWindowViewModel mainWindowViewModel)
        {
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);

            mainWindowViewModel.SelectedFilterFromComboBox = new VideoFilter()
            {
                FilterType = FilterType.PlaylistTitle,
                Title = playlistData.Title
            };

            mainWindowViewModel.Playlists.Add(playlistData);
            
            playlistData.PlaylistItems = new List<PlaylistItemData>() {new() {VideoId = videoData.Id}};
            youTubeCleanupToolDbContext.GetPlaylists().Returns(new List<PlaylistData> {playlistData});

            youTubeCleanupToolDbContext.GetVideos().Returns(new List<VideoData>() { videoData });
            
            mainWindowViewModel.Videos.Should().BeEmpty();
            await mainWindowViewModel.LoadData();

            var expectedVideos = new List<WpfVideoData> {mapper.Map<WpfVideoData>(videoData)};
            mainWindowViewModel.Videos.Should().BeEquivalentTo(expectedVideos, WithDefaultListCompare());
            
            await mainWindowViewModel.LoadData();

            mainWindowViewModel.Videos.Should().BeEquivalentTo(expectedVideos, WithDefaultListCompare());
        }

        [Theory, AutoNSubstituteData]
        public async Task When_video_added_to_playlist_on_YouTube_and_refresh_happens_Then_video_inserted_into_ui_in_the_correct_location(
            [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            WpfPlaylistData playlistData,
            [NoAutoProperties] MainWindowViewModel mainWindowViewModel)
        {
            mainWindowViewModel.ShouldSelectingFilterUpdateVideos = false;
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);

            var videoData = new VideoData {Id = "1", Title = "a"};
            var videoDataTwo = new VideoData {Id = "2", Title = "1"};
            var videoDataThree = new VideoData {Id = "3", Title = "c"};
            var videoDataFour = new VideoData {Id = "4", Title = "b"};

            mainWindowViewModel.SelectedFilterFromComboBox = new VideoFilter()
            {
                FilterType = FilterType.PlaylistTitle,
                Title = playlistData.Title
            };
            
            mainWindowViewModel.Playlists.Add(playlistData);

            playlistData.PlaylistItems = new List<PlaylistItemData> { new() { VideoId = videoData.Id }, new() { VideoId = videoDataTwo.Id }, new() { VideoId = videoDataThree.Id } };
            youTubeCleanupToolDbContext.GetPlaylists().Returns(new List<PlaylistData> { playlistData });

            youTubeCleanupToolDbContext.GetVideos().Returns(
                new List<VideoData> { videoData, videoDataTwo, videoDataThree }, 
                new List<VideoData> {videoData, videoDataTwo, videoDataThree, videoDataFour},
                new List<VideoData> {videoDataFour, videoDataTwo, videoData, videoDataThree, }
                );

            await mainWindowViewModel.LoadData();

            var expectedVideos = new List<WpfVideoData>
            {
                new() {Id = "2", Title = "1"},
                new() {Id = "1", Title = "a"},
                new() {Id = "3", Title = "c"},
            };
            LogVideos(mainWindowViewModel, expectedVideos);
            mainWindowViewModel.Videos.Should().BeEquivalentTo(expectedVideos, WithDefaultListCompare());

            playlistData.PlaylistItems.Add(new PlaylistItemData { VideoId = videoDataFour.Id});

            await mainWindowViewModel.LoadData();

            expectedVideos = new List<WpfVideoData>
            {
                new() {Id = "2", Title = "1"},
                new() {Id = "1", Title = "a"},
                new() {Id = "4", Title = "b"},
                new() {Id = "3", Title = "c"},
            };
            mainWindowViewModel.Videos.Should().BeEquivalentTo(expectedVideos, WithDefaultListCompare());
            
            LogVideos(mainWindowViewModel, expectedVideos);
            
            await mainWindowViewModel.LoadData();
            mainWindowViewModel.Videos.Should().BeEquivalentTo(expectedVideos, WithDefaultListCompare());
            
            LogVideos(mainWindowViewModel, expectedVideos);
        }

        private void LogVideos(MainWindowViewModel mainWindowViewModel, List<WpfVideoData> expectedVideos)
        {
            _testOutputHelper.WriteLine($"mainWindowViewModel.Videos: {JsonConvert.SerializeObject(mainWindowViewModel.Videos.Select(x => new {x.Id, x.Title}), Formatting.Indented)}");
            _testOutputHelper.WriteLine($"expected: {JsonConvert.SerializeObject(expectedVideos.Select(x => new {x.Id, x.Title}), Formatting.Indented)}");
        }

        private static Func<EquivalencyAssertionOptions<IData>, EquivalencyAssertionOptions<IData>> WithDefaultListCompare()
        {
            return options => options.WithStrictOrdering()
                .Including(y => y.Title)
                .Including(y => y.Id);
        }

        [Fact(Skip = "Not Implemented")]
        public async Task When_playlist_is_deleted_from_YouTube_Then_it_is_deleted_from_video_filter(
        )
        {
            await Task.Run(() => throw new NotImplementedException());
        }
        
        [Fact(Skip = "Not Implemented")]
        public async Task When_playlist_is_added_to_YouTube_Then_it_is_added_to_video_filter(
        )
        {
            await Task.Run(() => throw new NotImplementedException());
        }

        [Fact(Skip = "Not Implemented")]
        public async Task When_video_removed_from_playlist_on_YouTube_and_refresh_happens_Then_video_removed_from_ui()
        {
            await Task.Run(() => throw new NotImplementedException());
        }

    }
}
