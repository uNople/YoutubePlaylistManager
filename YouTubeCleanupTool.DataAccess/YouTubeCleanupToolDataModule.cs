using Autofac;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupTool.DataAccess;

public class YouTubeCleanupToolDataModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
            builder.RegisterType<YouTubeCleanupToolDbContextFactory>().As<IYouTubeCleanupToolDbContextFactory>();
        }
}