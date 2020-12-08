using System;
using System.Threading.Tasks;

namespace YoutubeCleanupTool.Interfaces
{
    public interface IYouTubeServiceCreator
    {
        Lazy<Task<IYouTubeServiceWrapper>> YouTubeServiceWrapper { get; set; }
    }
}