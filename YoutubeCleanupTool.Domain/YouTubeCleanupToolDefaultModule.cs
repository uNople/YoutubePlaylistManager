using Autofac;

namespace YouTubeCleanupTool.Domain
{
    public class YouTubeCleanupToolDefaultModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GetAndCacheYouTubeData>().As<IGetAndCacheYouTubeData>();
        }
    }
}
