using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.Model;

namespace YoutubeCleanupTool.DataAccess
{
    public class YoutubeCleanupToolDbContext : DbContext
    {
        public DbSet<PlaylistData> Playlists { get; set; }
        public DbSet<PlaylistItemData> PlaylistItems { get; set; }
        public DbSet<VideoData> Videos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Note: No need to do anything here because everything's attributed with [Table]
            // Only need to do this for indexes and more complicated things
        }
    }
}
