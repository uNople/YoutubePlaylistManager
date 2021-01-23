using AutoMapper;
using YouTubeCleanupTool.Domain;

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
