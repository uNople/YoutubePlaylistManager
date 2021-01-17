using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupTool.DataAccess
{
    public interface IYouTubeCleanupToolDbContextFactory
    {
        IYouTubeCleanupToolDbContext Create();
    }
}