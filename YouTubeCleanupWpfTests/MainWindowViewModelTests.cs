using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Xunit2;
using AutoMapper;
using FluentAssertions;
using NSubstitute;
using UnitTests.Common;
using Xunit;
using YouTubeCleanupTool.DataAccess;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.UnitTests
{
    public class MainWindowViewModelTests
    {
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
            });
            
            await mainWindowViewModel.LoadData();
            
            mainWindowViewModel.Playlists.Should().BeEquivalentTo(new List<WpfPlaylistData>
            {
                new() { Id = "1", Title = "a"},
                new() { Id = "3", Title = "b"},
                new() { Id = "2", Title = "c"}
            });
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
            });
        }
        
        [Theory, AutoNSubstituteData]
        public async Task When_video_added_to_playlist_on_YouTube_and_refresh_happens_Then_video_inserted_into_ui_in_the_correct_location()
        {

        }
        
        [Theory, AutoNSubstituteData]
        public async Task When_video_removed_from_playlist_on_YouTube_and_refresh_happens_Then_video_removed_from_ui()
        {

        }

    }
}
