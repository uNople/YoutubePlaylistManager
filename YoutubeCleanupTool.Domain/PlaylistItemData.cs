using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace YoutubeCleanupTool.Model
{
	[Table("PlaylistItems")]
	public class PlaylistItemData
	{
		[Key]
		public string VideoId { get; set; }
		public string Id { get; set; }
		public string Title { get; set; }
		public string VideoPublishedAt { get; set; }
		public long? Position { get; set; }
		public string Kind { get; set; }
		public string VideoKind { get; set; }
		public string ThumbnailUrl { get; set; }
		public string PrivacyStatus { get; set; }
		public string AddedToPlaylist { get; set; }

		private DateTime? _videoPublishedAtDate;
		public DateTime? VideoPublishedAtDate
		{
			get
			{

				if (DateTime.TryParse(VideoPublishedAt, out var videoPublishedAtDate))
				{
					_videoPublishedAtDate = videoPublishedAtDate;
				}

				return _videoPublishedAtDate;
			}
		}

		private DateTime? _addedToPlaylistDate;
		public DateTime? AddedToPlaylistDate
		{
			get
			{

				if (DateTime.TryParse(AddedToPlaylist, out var addedToPlaylistDate))
				{
					_addedToPlaylistDate = addedToPlaylistDate;
				}

				return _addedToPlaylistDate;
			}
		}
	}
}
