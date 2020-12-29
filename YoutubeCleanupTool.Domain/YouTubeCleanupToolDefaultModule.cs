using System.Net.Http;
using Autofac;

namespace YouTubeCleanupTool.Domain
{
    public class YouTubeCleanupToolDefaultModule : Module
    {
        private static readonly HttpClient HttpClient = new HttpClient();
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GetAndCacheYouTubeData>().As<IGetAndCacheYouTubeData>();
            builder.RegisterInstance(HttpClient);
        }
    }
}
