using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Google.Apis.YouTube.v3.Data;
using YouTubeCleanupTool.Domain.Entities;
using Thumbnail = YouTubeCleanupTool.Domain.Entities.Thumbnail;

namespace YouTubeApiWrapper;

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
                .ForPath(videoData => videoData.ChannelId, videoData => videoData.MapFrom(video => video.Snippet.ChannelId))
                .ForPath(videoData => videoData.ChannelTitle, videoData => videoData.MapFrom(video => video.Snippet.ChannelTitle))
                .ForPath(videoData => videoData.License, videoData => videoData.MapFrom(video => video.Status.License))
                .ForPath(videoData => videoData.RecordingDate, videoData => videoData.MapFrom(video => video.RecordingDetails.RecordingDate))
                .ForPath(videoData => videoData.Thumbnails, videoData => videoData.MapFrom(video => MapThumbnails(video)))
                .ReverseMap();

            CreateMap<PlaylistItem, PlaylistItemData>()
                .ForPath(playlistItemData => playlistItemData.VideoId, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.ResourceId.VideoId))
                .ForPath(playlistItemData => playlistItemData.VideoPublishedAt, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.ContentDetails.VideoPublishedAt))
                .ForPath(playlistItemData => playlistItemData.Position, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.Position))
                .ForPath(playlistItemData => playlistItemData.ThumbnailUrl, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.Thumbnails.Default__.Url))
                .ForPath(playlistItemData => playlistItemData.PrivacyStatus, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Status.PrivacyStatus))
                .ForPath(playlistItemData => playlistItemData.AddedToPlaylist, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.PublishedAt))
                .ForPath(playlistItemData => playlistItemData.Title, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.Title))
                .ForPath(playlistItemData => playlistItemData.PlaylistDataId, playlistItemData => playlistItemData.MapFrom(playlistItem => playlistItem.Snippet.PlaylistId))
                .ReverseMap();

            CreateMap<PlaylistItemData, PlaylistItemData>();

            // NOTE: Commented these out for now cause I need to test what happens with deleted videos when we have this mapping here
            //CreateMap<PlaylistData, PlaylistData>();
            //CreateMap<VideoData, VideoData>();
        }

    public List<Thumbnail> MapThumbnails(Video video)
    {
            // Ugly (and the return), but... the video thumbnails can return null for any of these
            // so this is a bit tidier than null checking before adding to the collection
            Thumbnail map(Google.Apis.YouTube.v3.Data.Thumbnail thumb) => thumb == null ? null : new()
            {
                ThumbnailUrl = thumb.Url,
                Width = thumb.Width,
                Height = thumb.Height,
            };
            
            List<Thumbnail> thumbnails = new()
            {
                map(video.Snippet?.Thumbnails?.High),
                map(video.Snippet?.Thumbnails?.Maxres),
                map(video.Snippet?.Thumbnails?.Medium),
                map(video.Snippet?.Thumbnails?.Standard)
            };

            return thumbnails.Where(x => x != null).ToList();
        }

    private static List<Category> MapCategories(Video video)
    {
            return video.TopicDetails?.TopicCategories?.Select(x => new Category {CategoryName = x}).ToList() ?? new List<Category>();
        }
}