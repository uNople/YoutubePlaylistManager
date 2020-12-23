using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YoutubeCleanupTool.Domain;

namespace YoutubeCleanupTool.DataAccess
{
    public class YoutubeCleanupToolDbContext : DbContext, IYouTubeCleanupToolDbContext
    {
        private readonly IMapper _mapper;

        public YoutubeCleanupToolDbContext([NotNull] DbContextOptions options,
            IMapper mapper) : base(options)
        {
            _mapper = mapper;
        }

        public DbSet<PlaylistData> Playlists { get; set; }
        public DbSet<PlaylistItemData> PlaylistItems { get; set; }
        public DbSet<VideoData> Videos { get; set; }

        public async Task<List<PlaylistData>> GetPlaylists() => await Playlists.ToListAsync();
        public async Task<List<PlaylistItemData>> GetPlaylistItems() => await PlaylistItems.ToListAsync();
        public async Task<List<VideoData>> GetVideos() => await Videos.ToListAsync();

        public async Task<InsertStatus> UpsertPlaylist(PlaylistData data)
        {
            InsertStatus status;
            var existing = await Playlists.FindAsync(data.Id);
            if (existing != null)
            {
                status = InsertStatus.Updated;
                _mapper.Map(data, existing);
            }
            else
            {
                status = InsertStatus.Inserted;
                Playlists.Add(data);
            }
            return status;
        }

        public async Task<InsertStatus> UpsertPlaylistItem(PlaylistItemData data)
        {
            InsertStatus status;
            var existing = await PlaylistItems.FindAsync(data.Id);
            if (existing != null)
            {
                status = InsertStatus.Updated;
                _mapper.Map(data, existing);
            }
            else
            {
                status = InsertStatus.Inserted;
                PlaylistItems.Add(data);
            }
            return status;
        }

        public async Task<InsertStatus> UpsertVideo(VideoData data)
        {
            InsertStatus status;
            var existing = await Videos.FindAsync(data.Id);
            if (existing != null)
            {
                status = InsertStatus.Updated;
                _mapper.Map(data, existing);
            }
            else
            {
                status = InsertStatus.Inserted;
                Videos.Add(data);
            }
            return status;
        }

        public void Migrate()
        {
            Database.Migrate();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Note: No need to do anything here because everything's attributed with [Table]
            // Only need to do this for indexes and more complicated things

            base.OnModelCreating(modelBuilder);
        }
    }
}
