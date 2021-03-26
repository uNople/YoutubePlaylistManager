using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnitTests.Common;
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
                [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
                [Frozen] IYouTubeApi youTubeApi,
                [Frozen] IFixture fixture,
                GetAndCacheYouTubeData getAndCacheYouTubeData
            )
        {
            // TODO: add customization so we don't need to do this everywhere we have a DbContext and ContextFactory
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);
            var playlistItemData = fixture.CreateMany<PlaylistItemData>(3).ToList();
            youTubeCleanupToolDbContext.GetPlaylistItems().Returns(playlistItemData);
            var videoData = playlistItemData.Take(1).Select(x => new VideoData { Id = x.VideoId }).ToList();
            youTubeCleanupToolDbContext.GetVideos().Returns(videoData);

            // Make sure the videos aren't "deleted from YouTube"
            var videos = fixture.CreateMany<VideoData>(2).ToList();
            videos.ForEach(x => x.IsDeletedFromYouTube = false);

            youTubeApi.GetVideos(Arg.Any<List<string>>()).Returns(TestExtensions.ToAsyncEnumerable(videos));

            // Act
            await getAndCacheYouTubeData.GetVideos(Callback, false, CancellationToken.None);

            // Assert
            // TODO: Assert we're passing in expectedGetTheseVideos to GetVideos 
            await foreach (var _ in youTubeApi.Received(1).GetVideos(Arg.Any<List<string>>())) { }
            await youTubeCleanupToolDbContext.Received(2).UpsertVideo(Arg.Any<VideoData>());
            await youTubeCleanupToolDbContext.Received(2).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_all_videos_Then_YouTube_api_is_called_to_get_the_data_for_everything(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
                [Frozen] IYouTubeApi youTubeApi,
                [Frozen] IFixture fixture,
                GetAndCacheYouTubeData getAndCacheYouTubeData
            )
        {
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);
            var playlistItemData = fixture.CreateMany<PlaylistItemData>(3).ToList();
            youTubeCleanupToolDbContext.GetPlaylistItems().Returns(playlistItemData);
            var videoData = playlistItemData.Take(1).Select(x => new VideoData { Id = x.VideoId }).ToList();
            youTubeCleanupToolDbContext.GetVideos().Returns(videoData);

            // Make sure the videos aren't "deleted from YouTube"
            var videos = fixture.CreateMany<VideoData>(3).ToList();
            videos.ForEach(x => x.IsDeletedFromYouTube = false);

            youTubeApi.GetVideos(Arg.Any<List<string>>()).Returns(TestExtensions.ToAsyncEnumerable(videos));

            // Act
            await getAndCacheYouTubeData.GetVideos(Callback, true, CancellationToken.None);

            // Assert
            await foreach (var _ in youTubeApi.Received(1).GetVideos(Arg.Any<List<string>>())) { }
            await youTubeCleanupToolDbContext.Received(3).UpsertVideo(Arg.Any<VideoData>());
            await youTubeCleanupToolDbContext.Received(3).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_playlists_from_YouTube_api_Then_they_are_saved_to_the_database(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
                [Frozen] IYouTubeApi youTubeApi,
                List<PlaylistData> playlistData,
                GetAndCacheYouTubeData getAndCacheYouTubeData)
        {
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);
            youTubeApi.GetPlaylists().Returns(TestExtensions.ToAsyncEnumerable(playlistData));

            // Act
            await getAndCacheYouTubeData.GetPlaylists(Callback, CancellationToken.None);

            // Assert
            await foreach (var _ in youTubeApi.Received(1).GetPlaylists()) { }
            await youTubeCleanupToolDbContext.Received(3).UpsertPlaylist(Arg.Any<PlaylistData>());
            await youTubeCleanupToolDbContext.Received(1).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_playlistItems_from_YouTube_api_Then_they_are_saved_to_the_database(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
                [Frozen] IYouTubeApi youTubeApi,
                PlaylistData playlist,
                List<PlaylistItemData> playlistItemData,
                GetAndCacheYouTubeData getAndCacheYouTubeData)
        {
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);
            youTubeCleanupToolDbContext.GetPlaylists().Returns(new List<PlaylistData> {playlist});
            youTubeApi.GetPlaylistItems(Arg.Any<string>(), Arg.Any<Func<string, Task>>()).Returns(TestExtensions.ToAsyncEnumerable(playlistItemData));

            youTubeCleanupToolDbContext.GetPlaylistItems(Arg.Any<string>()).Returns(new List<PlaylistItemData>());

            // Act
            await getAndCacheYouTubeData.GetPlaylistItems(Callback, CancellationToken.None);

            // Assert
            await foreach (var _ in youTubeApi.Received(1).GetPlaylistItems(Arg.Any<string>(), Arg.Any<Func<string, Task>>())) { }
            await youTubeCleanupToolDbContext.Received(3).UpsertPlaylistItem(Arg.Any<PlaylistItemData>());
            await youTubeCleanupToolDbContext.Received(1).SaveChangesAsync();
        }
    
        [Theory, AutoNSubstituteData]
        public async Task When_getting_playlistItems_from_YouTube_api_Then_playlists_which_no_longer_exist_get_deleted_from_db(
            [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
            [Frozen] IYouTubeCleanupToolDbContextFactory youTubeCleanupToolDbContextFactory,
            [Frozen] IYouTubeApi youTubeApi,
            PlaylistData playlist,
            
            GetAndCacheYouTubeData getAndCacheYouTubeData)
        {
            var playlistItems = new List<PlaylistItemData>
            {
                new()
                {
                    Id = "1",
                    Title = "a",
                    PlaylistDataId = playlist.Id
                },
                new()
                {
                    Id = "2",
                    Title = "b",
                    PlaylistDataId = playlist.Id
                }
            };
            youTubeCleanupToolDbContextFactory.Create().Returns(youTubeCleanupToolDbContext);
            youTubeCleanupToolDbContext.GetPlaylists().Returns(new List<PlaylistData> {playlist});
            youTubeApi.GetPlaylistItems(Arg.Any<string>(), Arg.Any<Func<string, Task>>()).Returns(TestExtensions.ToAsyncEnumerable(playlistItems));
            
            var originalPlaylistItems = new List<PlaylistItemData>
            {
                new()
                {
                    Id = "1",
                    Title = "a",
                    PlaylistDataId = playlist.Id
                },
                new()
                {
                    Id = "2",
                    Title = "b",
                    PlaylistDataId = playlist.Id
                },
                new()
                {
                    Id = "3",
                    Title = "c",
                    PlaylistDataId = playlist.Id
                }
            };
            youTubeCleanupToolDbContext.GetPlaylistItems(Arg.Any<string>()).Returns(originalPlaylistItems);

            await getAndCacheYouTubeData.GetPlaylistItems(Callback, CancellationToken.None);
            
            await foreach (var _ in youTubeApi.Received(1).GetPlaylistItems(Arg.Any<string>(), Arg.Any<Func<string, Task>>())) { }
            await youTubeCleanupToolDbContext.Received(2).UpsertPlaylistItem(Arg.Any<PlaylistItemData>());
            youTubeCleanupToolDbContext.Received(1).RemovePlaylistItem(Arg.Any<PlaylistItemData>());
            await youTubeCleanupToolDbContext.Received(1).SaveChangesAsync();
        }
        
        private Task Callback(IData data, InsertStatus insertStatus, CancellationToken cancellationToken)
        {
            _testOutputHelper.WriteLine($"{data.Title} - {insertStatus}");
            return Task.CompletedTask;
        }
    }
}
