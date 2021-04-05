using AutoMapper;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupWpf
{
    public class WpfYouTubeMapperProfile : Profile
    {
        public WpfYouTubeMapperProfile()
        {
            CreateMap<WpfVideoData, VideoData>()
                .ReverseMap();

            CreateMap<WpfPlaylistData, PlaylistData>()
                .ReverseMap();
        }
    }
}
