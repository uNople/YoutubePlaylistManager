using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Google.Apis.YouTube.v3.Data;
using YouTubeCleanupTool.Domain;

namespace YouTubeApiWrapper
{
    public class YouTubeObjectsMapperProfile : Profile
	{
		public YouTubeObjectsMapperProfile()
        {
            CreateMap<Playlist, PlaylistData>()
                .ForPath(playlistData => playlistData.Title, playlistData => playlistData.MapFrom(playlist => playlist.Snippet.Localized.Title))
                .ForPath(playlistData => playlistData.PrivacyStatus, playlistData => playlistData.MapFrom(playlist => playlist.Status.PrivacyStatus))
                .ForPath(playlistData => playlistData.ThumbnailUrl, playlistData => playlistData.MapFrom(playlist => playlist.Snippet.Thumbnails.Default__.Url))
                .ReverseMap();

            CreateMap<Video, VideoData>()
                .ForPath(videoData => videoData.Categories, videoData => videoData.MapFrom(video => MapCategories(video)))
                .ForPath(videoData => videoData.CategoryId, videoData => videoData.MapFrom(video => int.Parse(video.Snippet.CategoryId)))
                .ForPath(videoData => videoData.ThumbnailUrl, videoData => videoData.MapFrom(video => video.Snippet.Thumbnails.Default__.Url))
                .ForPath(videoData => videoData.Description, videoData => videoData.MapFrom(video => video.Snippet.Description))
                .ForPath(videoData => videoData.Title, videoData => videoData.MapFrom(video => video.Snippet.Title))
                .ForPath(videoData => videoData.License, videoData => videoData.MapFrom(video => video.Status.License))
                .ForPath(videoData => videoData.RecordingDate, videoData => videoData.MapFrom(video => video.RecordingDetails.RecordingDate))
                .ReverseMap();

            CreateMap<PlaylistItem, PlaylistItemData>()
                .ForPath(playlistItemData => playlistItemData.VideoId, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.ContentDetails.VideoId))
                .ForPath(playlistItemData => playlistItemData.VideoPublishedAt, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.ContentDetails.VideoPublishedAt))
                .ForPath(playlistItemData => playlistItemData.Position, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.Position))
                .ForPath(playlistItemData => playlistItemData.ThumbnailUrl, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.Thumbnails.Default__.Url))
                .ForPath(playlistItemData => playlistItemData.PrivacyStatus, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Status.PrivacyStatus))
                .ForPath(playlistItemData => playlistItemData.AddedToPlaylist, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.PublishedAt))
                .ForPath(playlistItemData => playlistItemData.Title, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.Title))
                .ReverseMap();
        }

        private static List<Category> MapCategories(Video video)
        {
            return video.TopicDetails?.TopicCategories?.Select(x => new Category { CategoryName = x }).ToList() ?? new List<Category>();
        }
    }
}
