using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Threading.Tasks;

namespace YoutubeCleanupTool
{
    public interface IYouTubeServiceCreator
    {
        Task<YouTubeService> CreateYouTubeService(string googleApiKeyCredentialName, string clientSecretPath, string fileDataStoreName);
    }
}