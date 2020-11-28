using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Apis.YouTube.v3.ActivitiesResource;

namespace YoutubeCleanupTool
{
    public class YouTubeServiceRequestWrapper
    {
        public static async Task<List<TResult>> GetResults<TResult>(dynamic request)
        {
            var result = new List<TResult>();
            request.MaxResults = 50;
            var response = await request.ExecuteAsync();
            result.AddRange(response.Items);

            while (response.NextPageToken != null)
            {
                Console.Write(".");
                request.PageToken = response.NextPageToken;
                response = await request.ExecuteAsync();
                result.AddRange(response.Items);
            }

            return result;
        }
    }
}
