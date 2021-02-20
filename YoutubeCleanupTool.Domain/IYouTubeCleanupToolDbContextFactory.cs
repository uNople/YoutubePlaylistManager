namespace YouTubeCleanupTool.Domain
{
    public interface IYouTubeCleanupToolDbContextFactory
    {
        IYouTubeCleanupToolDbContext Create();
    }
}