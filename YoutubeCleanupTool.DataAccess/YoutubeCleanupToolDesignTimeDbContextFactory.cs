using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.DataAccess;

namespace YoutubeCleanupConsole
{
    public class YoutubeCleanupToolDesignTimeDbContextFactory : IDesignTimeDbContextFactory<YoutubeCleanupToolDbContext>
    {
        public YoutubeCleanupToolDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<YoutubeCleanupToolDbContext>();

            builder.UseSqlite();

            return new YoutubeCleanupToolDbContext(builder.Options);
        }
    }
}
