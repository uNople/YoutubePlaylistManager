﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace YouTubeCleanupTool.Domain
{
    [Table("Videos")]
    public class VideoData : IData
    {
        [Key]
        public string Id { get; set; }
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Description { get; set; }
        public string License { get; set; } // doesn't seem to indicate music. Only "YouTube" and "creativeCommon" seen
        public string RecordingDate { get; set; }
        public int CategoryId { get; set; }
        public List<Category> Categories { get; set; }
        public bool IsDeletedFromYouTube { get; set; }
        public bool IsMusicByCategoryId => CategoryId == 10;
        public bool IsMusicByDescription => Description.StartsWithCi("Provided to YouTube by ") && Description.EndsWithCi("Auto-generated by YouTube.");
        public bool IsMusicByCategoryLink => Categories?.Any(x => x.CategoryName.EndsWithCi("music")) ?? false;
        public bool CouldBeMusic => IsMusicByCategoryId || IsMusicByCategoryLink || IsMusicByDescription;
    }

}
