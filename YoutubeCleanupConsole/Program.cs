using Autofac;
using System;
using System.Threading.Tasks;
using YoutubeCleanupTool;

namespace YoutubeCleanupConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<YouTubeCleanupToolModule>();
            builder.RegisterModule<YoutubeCleanupConsoleModule>();
            var container = builder.Build();
            try
            {
                await container.Resolve<IConsoleUi>().Run();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex}");
                Console.WriteLine("Press enter to exit");
                Console.ReadLine();
            }
        }
    }
}
