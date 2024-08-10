using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace YouTubeCleanupTool.DataAccess;

public class YouTubeCleanupToolDesignTimeDbContextFactory : IDesignTimeDbContextFactory<YouTubeCleanupToolDbContext>
{
    public YouTubeCleanupToolDbContext CreateDbContext(string[] args)
    {
            var builder = new DbContextOptionsBuilder<YouTubeCleanupToolDbContext>();

            builder.UseSqlite();

            return new YouTubeCleanupToolDbContext(builder.Options, null);
        }
}