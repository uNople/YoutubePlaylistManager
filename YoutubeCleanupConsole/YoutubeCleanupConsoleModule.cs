using Autofac;

namespace YouTubeCleanupConsole
{
    public class YouTubeCleanupConsoleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleUi>().As<IConsoleUi>();
            builder.RegisterType<ConsoleDisplayParams>();
        }
    }
}
