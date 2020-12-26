using Autofac;

namespace YouTubeCleanupConsole
{
    public class YoutubeCleanupConsoleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleUi>().As<IConsoleUi>();
            builder.RegisterType<ConsoleDisplayParams>();
        }
    }
}
