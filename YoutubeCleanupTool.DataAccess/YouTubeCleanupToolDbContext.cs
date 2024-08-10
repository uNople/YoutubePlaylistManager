using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupTool.Domain.Entities;

namespace YouTubeCleanupTool.DataAccess;

public class YouTubeCleanupToolDbContext(
    [NotNull] DbContextOptions options,
    IMapper mapper) : DbContext(options), IYouTubeCleanupToolDbContext
{
    private DbSet<PlaylistData> Playlists { get; set; }
    private DbSet<PlaylistItemData> PlaylistItems { get; set; }

    private DbSet<VideoData> Videos { get; set; }

    // These methods exist so that our interface doesn't pull in DbSet, or anything EF core related
    public async Task<List<PlaylistData>> GetPlaylists() => await Playlists.Include(x => x.PlaylistItems).ToListAsync();
    public async Task<List<PlaylistItemData>> GetPlaylistItems() => await PlaylistItems.ToListAsync();

    public async Task<List<PlaylistItemData>> GetPlaylistItems(string playlistId) =>
        await PlaylistItems.Where(x => x.PlaylistDataId == playlistId).ToListAsync();

    public async Task<List<VideoData>> GetVideos() => await Videos.ToListAsync();
    public async Task<List<string>> GetVideoTitles() => await Videos.Select(x => x.Title).ToListAsync();
    public async Task<bool> VideoExists(string id) => await Videos.FindAsync(id) != null;
    public async Task<InsertStatus> UpsertPlaylist(PlaylistData data) => await Upsert(Playlists, data);
    public async Task<InsertStatus> UpsertPlaylistItem(PlaylistItemData data) => await Upsert(PlaylistItems, data);
    public async Task<InsertStatus> UpsertVideo(VideoData data) => await Upsert(Videos, data);

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

    public async Task<PlaylistItemData> GetPlaylistItem(string playlistId, string videoId)
    {
        return await PlaylistItems.FirstOrDefaultAsync(x => x.PlaylistDataId == playlistId && x.VideoId == videoId);
    }

    public void RemovePlaylistItem(PlaylistItemData playlistItem) => PlaylistItems.Remove(playlistItem);

    public void RemovePlaylist(string playlistId)
    {
        var playlistToRemove = Playlists.FirstOrDefault(x => x.Id == playlistId);
        if (playlistToRemove != null)
            Playlists.Remove(playlistToRemove);
    }

    public async Task<List<VideoData>> GetUncategorizedVideos(List<string> playlistTitles)
    {
        var playlists = (await Playlists
                .Where(x => playlistTitles.Contains(x.Title))
                .ToListAsync())
            .Select(x => x.Id)
            .ToList();

        var videosOnlyInOnePlaylist = new HashSet<string>(
            PlaylistItems
                .AsEnumerable()
                .GroupBy(x => x.VideoId)
                .Where(x => x.Count() <= 1)
                .Select(x => new { VideoId = x.Key, PlaylistId = x.First().PlaylistDataId })
                .Where(x => playlists.Any(y => x.PlaylistId == y))
                .Select(x => x.VideoId)
        );

        return await Videos.Where(x => videosOnlyInOnePlaylist.Contains(x.Id))
            .OrderBy(x => x.Title)
            .ToListAsync();
    }

    public void Migrate()
    {
        Database.Migrate();
    }

    private async Task<InsertStatus> Upsert<T>(DbSet<T> dbSet, T data) where T : class, IData
    {
        InsertStatus status;
        var existing = await dbSet.FindAsync(data.Id);
        if (existing != null)
        {
            status = InsertStatus.Updated;
            mapper.Map(data, existing);
        }
        else
        {
            status = InsertStatus.Inserted;
            await dbSet.AddAsync(data);
        }

        return status;
    }
}