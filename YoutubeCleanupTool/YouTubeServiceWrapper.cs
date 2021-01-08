using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using YouTubeApiWrapper.Interfaces;

namespace YouTubeApiWrapper
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

            // force-get hardcoded playlist names (liked, liked music, Could be others. Watch later [WL] can't be gotten through the api)
            playlistRequest = Playlists.List("contentDetails,id,snippet,status");
            result.AddRange(await GetAnotherPlaylist(playlistRequest, "LL"));
            result.AddRange(await GetAnotherPlaylist(playlistRequest, "LM"));

            return result;
        }

        private static async Task<List<Playlist>> GetAnotherPlaylist(PlaylistsResource.ListRequest playlistRequest, string playlistName)
        {
            playlistRequest.Id = playlistName;
            return await HandlePagination<Playlist>(playlistRequest);
        }

        public async Task<PlaylistItem> AddVideoToPlaylist(string playlistId, string videoId)
        {
            var playlistItem = new PlaylistItem
            {
                Snippet = new PlaylistItemSnippet
                {
                    PlaylistId = playlistId,
                    ResourceId = new ResourceId
                    {
                        Kind = "youtube#video",
                        VideoId = videoId
                    }
                }
            };

            return await PlaylistItems.Insert(playlistItem, "snippet").ExecuteAsync();
        }

        public async Task RemoveVideoFromPlaylist(string playlistItemId)
        {
            await PlaylistItems.Delete(playlistItemId).ExecuteAsync();
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
