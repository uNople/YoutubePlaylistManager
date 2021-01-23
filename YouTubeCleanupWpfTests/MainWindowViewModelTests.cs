using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using UnitTests.Common;
using Xunit;
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
    }
}
