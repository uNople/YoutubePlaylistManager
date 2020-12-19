using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace YoutubeCleanupTool.Model
{
	[Table("Playlists")]
	public class PlaylistData
	{
		[Key]
		public string Id { get; set; }
		public string Title { get; set; }
		public string PrivacyStatus { get; set; }
		public string Kind { get; set; }
		public string ThumbnailUrl { get; set; }
	}
}
