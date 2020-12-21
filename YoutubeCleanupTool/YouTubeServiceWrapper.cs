using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class YouTubeServiceWrapper : YouTubeService, IYouTubeServiceWrapper
    {
        public YouTubeServiceWrapper(Initializer initializer) : base(initializer)
        {
        }

        public async Task<List<Video>> GetVideos(string id)
        {
            // https://developers.google.com/youtube/v3/docs/videos/list
            // playlist LL and LM to get liked videos / liked music
            var items = Videos.List("contentDetails,id,snippet,status,player,projectDetails,recordingDetails,statistics,topicDetails");
            items.Id = id;
            return await HandlePagination<Video>(items);
        }

        public async Task<List<PlaylistItem>> GetPlaylistItems(string playlistId)
        {
            // https://developers.google.com/youtube/v3/docs/playlistItems/list
            var playlistItems = PlaylistItems.List("contentDetails,id,snippet,status");
            playlistItems.PlaylistId = playlistId;
            return await HandlePagination<PlaylistItem>(playlistItems);
        }

        public async Task<List<Playlist>> GetPlaylists()
        {
            // auditDetails requires youtubepartner-channel-audit scope
            // brandingSettings, contentOwnerDetails requires something?
            // statistics topicDetails
            // Don't care about: localizations (even though I can get it)
            var playlistRequest = Playlists.List("contentDetails,id,snippet,status");
            playlistRequest.Mine = true;
            var result = await HandlePagination<Playlist>(playlistRequest);

            // force-get LL and LM playlists
            playlistRequest = Playlists.List("contentDetails,id,snippet,status");
            playlistRequest.Id = "LL";
            result.AddRange(await HandlePagination<Playlist>(playlistRequest));
            playlistRequest.Id = "LM";
            result.AddRange(await HandlePagination<Playlist>(playlistRequest));

            return result;
        }

        private static async Task<List<TResult>> HandlePagination<TResult>(dynamic request)
        {
            var result = new List<TResult>();
            request.MaxResults = 50;
            var response = await request.ExecuteAsync();
            result.AddRange(response.Items);

            while (response.NextPageToken != null)
            {
                request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync();
                result.AddRange(response.Items);
            }

            return result;
        }
    }
}
