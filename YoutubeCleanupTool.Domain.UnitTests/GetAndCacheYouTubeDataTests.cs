using AutoFixture;
using AutoFixture.Xunit2;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace YoutubeCleanupTool.Domain.UnitTests
{
    public class GetAndCacheYouTubeDataTests
    {
        [Theory, AutoNSubstituteData]
        public async Task When_only_getting_videos_that_dont_already_exist_Then_youtube_api_is_not_called_to_get_the_data(
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

            // Make sure the videos aren't "deleted from youtube"
            var videos = fixture.CreateMany<VideoData>(2).ToList();
            videos.ForEach(x => x.IsDeletedFromYouTube = false);

            youTubeApi.GetVideos(Arg.Any<List<string>>()).Returns(videos.ToAsyncEnumerable());

            var callback = new Action<VideoData, InsertStatus>((data, insertStatus) => Console.WriteLine(data.Title));

            // Act
            await getAndCacheYouTubeData.GetVideos(callback, false);

            // Assert
            var expectedGetTheseVideos = playlistItemData.Where(x => !videoData.Any(y => y.Id == x.VideoId)).Select(x => x.VideoId).ToList();
            // TODO: Assert we're passing in expectedGetTheseVideos to GetVideos 
            youTubeApi.Received(1).GetVideos(Arg.Any<List<string>>());
            await youTubeCleanupToolDbContext.Received(2).UpsertVideo(Arg.Any<VideoData>());
            await youTubeCleanupToolDbContext.Received(2).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_all_videos_Then_youtube_api_is_called_to_get_the_data_for_everything(
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

            // Make sure the videos aren't "deleted from youtube"
            var videos = fixture.CreateMany<VideoData>(3).ToList();
            videos.ForEach(x => x.IsDeletedFromYouTube = false);

            youTubeApi.GetVideos(Arg.Any<List<string>>()).Returns(videos.ToAsyncEnumerable());

            var callback = new Action<VideoData, InsertStatus>((data, insertStatus) => Console.WriteLine(data.Title));

            // Act
            await getAndCacheYouTubeData.GetVideos(callback, true);

            // Assert
            youTubeApi.Received(1).GetVideos(Arg.Any<List<string>>());
            await youTubeCleanupToolDbContext.Received(3).UpsertVideo(Arg.Any<VideoData>());
            await youTubeCleanupToolDbContext.Received(3).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_playlists_from_youtube_api_Then_they_are_saved_to_the_database(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeApi youTubeApi,
                List<PlaylistData> playlistdata,
                GetAndCacheYouTubeData getAndCacheYouTubeData)
        {
            youTubeApi.GetPlaylists().Returns(playlistdata.ToAsyncEnumerable());

            var callback = new Action<PlaylistData, InsertStatus>((data, insertStatus) => Console.WriteLine(data.Title));

            // Act
            await getAndCacheYouTubeData.GetPlaylists(callback);

            // Assert
            youTubeApi.Received(1).GetPlaylists();
            await youTubeCleanupToolDbContext.Received(3).UpsertPlaylist(Arg.Any<PlaylistData>());
            await youTubeCleanupToolDbContext.Received(1).SaveChangesAsync();
        }

        [Theory, AutoNSubstituteData]
        public async Task When_getting_playlistItems_from_youtube_api_Then_they_are_saved_to_the_database(
                [Frozen] IYouTubeCleanupToolDbContext youTubeCleanupToolDbContext,
                [Frozen] IYouTubeApi youTubeApi,
                List<PlaylistItemData> playlistItemData,
                GetAndCacheYouTubeData getAndCacheYouTubeData)
        {
            youTubeApi.GetPlaylistItems(Arg.Any<List<PlaylistData>>()).Returns(playlistItemData.ToAsyncEnumerable());

            var callback = new Action<PlaylistItemData, InsertStatus>((data, insertStatus) => Console.WriteLine(data.Title));

            // Act
            await getAndCacheYouTubeData.GetPlaylistItems(callback);

            // Assert
            youTubeApi.Received(1).GetPlaylistItems(Arg.Any<List<PlaylistData>>());
            await youTubeCleanupToolDbContext.Received(3).UpsertPlaylistItem(Arg.Any<PlaylistItemData>());
            await youTubeCleanupToolDbContext.Received(1).SaveChangesAsync();
        }
    }
}
