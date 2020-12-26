using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace YouTubeCleanupTool.Domain.UnitTests
{
    public class GetAndCacheYouTubeDataTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public GetAndCacheYouTubeDataTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory, AutoNSubstituteData]
        public async Task When_only_getting_videos_that_dont_already_exist_Then_YouTube_api_is_not_called_to_get_the_data(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeApi youTubeApi,
                [Frozen] IFixture fixture,
                GetAndCacheYouTubeData getAndCacheYouTubeData
            )
        {
            var playlistItemData = fixture.CreateMany<PlaylistItemData>(3).ToList();
            youTubeCleanupToolDbContext.GetPlaylistItems().Returns(playlistItemData);
            var videoData = playlistItemData.Take(1).Select(x => new VideoData { Id = x.VideoId }).ToList();
            youTubeCleanupToolDbContext.GetVideos().Returns(videoData);

            // Make sure the videos aren't "deleted from YouTube"
            var videos = fixture.CreateMany<VideoData>(2).ToList();
            videos.ForEach(x => x.IsDeletedFromYouTube = false);

            youTubeApi.GetVideos(Arg.Any<List<string>>()).Returns(videos.ToAsyncEnumerable());

            var callback = new Action<VideoData, InsertStatus>((data, insertStatus) => _testOutputHelper.WriteLine($"{data.Title} - {insertStatus}"));

            // Act
            await getAndCacheYouTubeData.GetVideos(callback, false, CancellationToken.None);

            // Assert
            // TODO: Assert we're passing in expectedGetTheseVideos to GetVideos 
            await foreach (var _ in youTubeApi.Received(1).GetVideos(Arg.Any<List<string>>())) { }
            await youTubeCleanupToolDbContext.Received(2).UpsertVideo(Arg.Any<VideoData>());
            await youTubeCleanupToolDbContext.Received(2).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_all_videos_Then_YouTube_api_is_called_to_get_the_data_for_everything(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeApi youTubeApi,
                [Frozen] IFixture fixture,
                GetAndCacheYouTubeData getAndCacheYouTubeData
            )
        {
            var playlistItemData = fixture.CreateMany<PlaylistItemData>(3).ToList();
            youTubeCleanupToolDbContext.GetPlaylistItems().Returns(playlistItemData);
            var videoData = playlistItemData.Take(1).Select(x => new VideoData { Id = x.VideoId }).ToList();
            youTubeCleanupToolDbContext.GetVideos().Returns(videoData);

            // Make sure the videos aren't "deleted from YouTube"
            var videos = fixture.CreateMany<VideoData>(3).ToList();
            videos.ForEach(x => x.IsDeletedFromYouTube = false);

            youTubeApi.GetVideos(Arg.Any<List<string>>()).Returns(videos.ToAsyncEnumerable());

            var callback = new Action<VideoData, InsertStatus>((data, insertStatus) => _testOutputHelper.WriteLine($"{data.Title} - {insertStatus}"));

            // Act
            await getAndCacheYouTubeData.GetVideos(callback, true, CancellationToken.None);

            // Assert
            await foreach (var _ in youTubeApi.Received(1).GetVideos(Arg.Any<List<string>>())) { }
            await youTubeCleanupToolDbContext.Received(3).UpsertVideo(Arg.Any<VideoData>());
            await youTubeCleanupToolDbContext.Received(3).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_playlists_from_YouTube_api_Then_they_are_saved_to_the_database(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeApi youTubeApi,
                List<PlaylistData> playlistData,
                GetAndCacheYouTubeData getAndCacheYouTubeData)
        {
            youTubeApi.GetPlaylists().Returns(playlistData.ToAsyncEnumerable());

            var callback = new Action<PlaylistData, InsertStatus>((data, insertStatus) => _testOutputHelper.WriteLine($"{data.Title} - {insertStatus}"));

            // Act
            await getAndCacheYouTubeData.GetPlaylists(callback);

            // Assert
            await foreach (var _ in youTubeApi.Received(1).GetPlaylists()) { }
            await youTubeCleanupToolDbContext.Received(3).UpsertPlaylist(Arg.Any<PlaylistData>());
            await youTubeCleanupToolDbContext.Received(1).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_playlistItems_from_YouTube_api_Then_they_are_saved_to_the_database(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeApi youTubeApi,
                List<PlaylistItemData> playlistItemData,
                GetAndCacheYouTubeData getAndCacheYouTubeData)
        {
            youTubeApi.GetPlaylistItems(Arg.Any<List<PlaylistData>>()).Returns(playlistItemData.ToAsyncEnumerable());

            var callback = new Action<PlaylistItemData, InsertStatus>((data, insertStatus) => _testOutputHelper.WriteLine($"{data.Title} - {insertStatus}"));

            // Act
            await getAndCacheYouTubeData.GetPlaylistItems(callback);

            // Assert
            await foreach (var _ in youTubeApi.Received(1).GetPlaylistItems(Arg.Any<List<PlaylistData>>())) { }
            await youTubeCleanupToolDbContext.Received(3).UpsertPlaylistItem(Arg.Any<PlaylistItemData>());
            await youTubeCleanupToolDbContext.Received(1).SaveChangesAsync();
        }
    }
}
