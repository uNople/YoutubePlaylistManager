using System.Threading.Tasks;

namespace YoutubeCleanupTool.Interfaces
{
    public interface IYouTubeServiceCreator
    {
        Task<IYouTubeServiceWrapper> CreateYouTubeService();
    }
}