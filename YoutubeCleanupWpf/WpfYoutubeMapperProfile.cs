using AutoMapper;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class WpfYoutubeMapperProfile : Profile
    {
        public WpfYoutubeMapperProfile()
        {
            CreateMap<WpfVideoData, VideoData>()
                .ReverseMap();

            CreateMap<WpfPlaylistData, PlaylistData>()
                .ReverseMap();
        }
    }
}
