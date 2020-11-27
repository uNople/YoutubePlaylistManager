using Google.Apis.Requests;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Google.Apis.YouTube.v3.ActivitiesResource;

namespace YoutubeCleanupTool
{
    public class YouTubeServiceRequestWrapper
    {
        public static List<TResult> GetResults<TResult>(dynamic request)
        {
            request.MaxResults = 50;
            var result = new List<TResult>();
            var response = request.Execute();
            result.AddRange(response.Items);

            while (response.NextPageToken != null)
            {
                request.PageToken = response.NextPageToken;
                response = request.Execute();
                result.AddRange(response.Items);
            }

            return result;
        }
    }
}
