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
        public async Task When_data_exists_in_playlists_and_update_happens_Then_new_data_is_inserted_in_the_right_place(
            [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            [Frozen] IMapper mapper,
            [NoAutoProperties]MainWindowViewModel mainWindowViewModel,
            IFixture fixture
            )
        {
            // kinda redonk - can I just wire up automapper in the test projects?
            mapper.Map<WpfPlaylistData>(Arg.Any<PlaylistData>())
                .Returns(x => new WpfPlaylistData() {Id = x.ArgAt<PlaylistData>(0).Id, Title = x.ArgAt<PlaylistData>(0).Title});

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

        public async Task When_data_exists_in_playlists_and_update_happens_Then_deleted_data_is_removed() {}
    }
}
