using AutoMapper;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.Model;

namespace YoutubeCleanupTool
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


		}
	}
}
