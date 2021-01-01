using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
