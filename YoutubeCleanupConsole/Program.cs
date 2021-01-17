using System;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using YouTubeApiWrapper;
using YouTubeCleanupTool.DataAccess;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupConsole
{
    class Program
    {
        static async Task Main()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<YouTubeApiWrapperModule>();
            builder.RegisterModule<YoutubeCleanupConsoleModule>();
            builder.RegisterModule<YouTubeCleanupToolDefaultModule>();
            builder.RegisterModule<YouTubeCleanupToolDataModule>();
            var container = builder.Build();
            try
            {
                container.Resolve<IYouTubeCleanupToolDbContextFactory>().Create().Migrate();
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
