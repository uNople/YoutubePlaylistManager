using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupTool.DataAccess
{
    public class YoutubeCleanupToolDbContext : DbContext, IYouTubeCleanupToolDbContext
    {
        private readonly IMapper _mapper;

        public YoutubeCleanupToolDbContext([NotNull] DbContextOptions options,
            IMapper mapper) : base(options)
        {
            _mapper = mapper;
        }

        private DbSet<PlaylistData> Playlists { get; set; }
        private DbSet<PlaylistItemData> PlaylistItems { get; set; }
        private DbSet<VideoData> Videos { get; set; }

        // These methods exist so that our interface doesn't pull in DbSet, or anything EF core related
        public async Task<List<PlaylistData>> GetPlaylists() => await Playlists.Include(x => x.PlaylistItems).ToListAsync();
        public async Task<List<PlaylistItemData>> GetPlaylistItems() => await PlaylistItems.ToListAsync();
        public async Task<List<VideoData>> GetVideos() => await Videos.ToListAsync();
        public async Task<List<string>> GetVideoTitles() => await Videos.Select(x => x.Title).ToListAsync();
        public async Task<bool> VideoExists(string id) => await Videos.FindAsync(id) != null;
        public async Task<InsertStatus> UpsertPlaylist(PlaylistData data) => await Upsert(Playlists, data);
        public async Task<InsertStatus> UpsertPlaylistItem(PlaylistItemData data) => await Upsert(PlaylistItems, data);
        public async Task<InsertStatus> UpsertVideo(VideoData data) => await Upsert(Videos, data);

        private async Task<InsertStatus> Upsert<T>(DbSet<T> dbSet, T data) where T : class, IData
        {
            InsertStatus status;
            var existing = await dbSet.FindAsync(data.Id);
            if (existing != null)
            {
                status = InsertStatus.Updated;
                _mapper.Map(data, existing);
            }
            else
            {
                status = InsertStatus.Inserted;
                dbSet.Add(data);
            }
            return status;
        }

        public async Task<List<IData>> FindAll(string regex)
        {
            var searchResults = new List<IData>();
            var searchTerm = new Regex(regex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            searchResults.AddRange(
                (await Videos
                    .ToListAsync())
                    .Where(x => searchTerm.IsMatch(x.Title))
                );

            searchResults.AddRange(
                    (await Playlists
                        .ToListAsync())
                        .Where(x => searchTerm.IsMatch(x.Title))
                );

            searchResults.AddRange(
                    (await PlaylistItems
                        .ToListAsync())
                        .Where(x => searchTerm.IsMatch(x.Title))
                );

            return searchResults;
        }

        public void Migrate()
        {
            Database.Migrate();
        }
    }
}
