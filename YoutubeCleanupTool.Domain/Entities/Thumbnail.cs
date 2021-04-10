using System.ComponentModel.DataAnnotations;

namespace YouTubeCleanupTool.Domain.Entities
{
    public class Thumbnail
    {
        [Key]
        public int Id { get; set; }
        public byte[] ThumbnailBytes { get; set; }
        public string ThumbnailUrl { get; set; }
        public long? Width { get; set; }
        public long? Height { get; set; }
    }
}