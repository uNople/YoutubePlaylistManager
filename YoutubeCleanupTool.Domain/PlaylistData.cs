﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YouTubeCleanupTool.Domain
{
    [Table("Playlists")]
    public class PlaylistData : IData
    {
        [Key]
        public string Id { get; set; }
        public string Title { get; set; }
        public string PrivacyStatus { get; set; }
        public string Kind { get; set; }
        public string ThumbnailUrl { get; set; }
        public List<PlaylistItemData> PlaylistItems { get; set; } = new();
    }
}
