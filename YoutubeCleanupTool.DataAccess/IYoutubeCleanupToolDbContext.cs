using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;
using YoutubeCleanupTool.Domain;

namespace YoutubeCleanupTool.DataAccess
{
    public interface IYoutubeCleanupToolDbContext
    {
        DbSet<PlaylistItemData> PlaylistItems { get; set; }
        DbSet<PlaylistData> Playlists { get; set; }
        DbSet<VideoData> Videos { get; set; }
        void Migrate();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}