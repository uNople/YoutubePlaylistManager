using Autofac;
using System;
using System.Threading.Tasks;
using YoutubeCleanupTool;

namespace YoutubeCleanupConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<YouTubeCleanupToolModule>();
            builder.RegisterModule<YoutubeCleanupConsoleModule>();
            var container = builder.Build();
            Task.WaitAll(container.Resolve<IConsoleUi>().Run());
        }
    }
}
