using FluentAssertions;
using Google.Apis.YouTube.v3.Data;
using Xunit;
using YouTubeApiWrapper;

namespace YouTubeApiWrapperTests;

public class YouTubeObjectsMapperProfileTests
{
    [Theory, AutoNSubstituteData]
    public void When_mapping_thumbnails_Then_empty_video_should_not_error(YouTubeObjectsMapperProfile youTubeObjectsMapperProfile)
    {
            var video = new Video();
            var thumbnails = youTubeObjectsMapperProfile.MapThumbnails(video);
            thumbnails.Should().BeEmpty();
        }

    [Theory, AutoNSubstituteData]
    public void When_mapping_thumbnails_Then_populating_all_thumbnails_should_map_all_thumbnails(YouTubeObjectsMapperProfile youTubeObjectsMapperProfile)
    {
            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Thumbnails = new ThumbnailDetails
                    {
                        High = new Thumbnail(),
                        Maxres = new Thumbnail(),
                        Medium = new Thumbnail(),
                        Standard = new Thumbnail()
                    }
                }
            };
            var thumbnails = youTubeObjectsMapperProfile.MapThumbnails(video);
            thumbnails.Count.Should().Be(4);
        }

    [Theory, AutoNSubstituteData]
    public void When_mapping_thumbnails_Then_populating_some_should_map_all_available_thumbnails(YouTubeObjectsMapperProfile youTubeObjectsMapperProfile)
    {
            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Thumbnails = new ThumbnailDetails
                    {
                        High = new Thumbnail(),
                        Standard = new Thumbnail()
                    }
                }
            };
            var thumbnails = youTubeObjectsMapperProfile.MapThumbnails(video);
            thumbnails.Count.Should().Be(2);
        }
}