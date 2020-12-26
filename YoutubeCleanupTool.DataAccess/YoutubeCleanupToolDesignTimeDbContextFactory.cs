using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YouTubeCleanupTool.DataAccess
{
    public class YoutubeCleanupToolDesignTimeDbContextFactory : IDesignTimeDbContextFactory<YoutubeCleanupToolDbContext>
    {
        public YoutubeCleanupToolDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<YoutubeCleanupToolDbContext>();

            builder.UseSqlite();

            return new YoutubeCleanupToolDbContext(builder.Options, null);
        }
    }
}
