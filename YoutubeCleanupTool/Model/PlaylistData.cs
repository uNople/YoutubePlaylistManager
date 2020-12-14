using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeCleanupTool.Model
{
	public class PlaylistData
	{
		public string Title { get; set; }
		public string Id { get; set; }
		public string PrivacyStatus { get; set; }
		public string Kind { get; set; }
		public string ThumbnailUrl { get; set; }
	}
}
