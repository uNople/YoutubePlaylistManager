using System.Net.Http;
using Autofac;

namespace YouTubeCleanupTool.Domain
{
    public class YouTubeCleanupToolDefaultModule : Module
    {
        private static readonly IHttpClientWrapper HttpClient = new HttpClientWrapper();
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GetAndCacheYouTubeData>().As<IGetAndCacheYouTubeData>();
            builder.RegisterInstance(HttpClient).As<IHttpClientWrapper>();
        }
    }
}
