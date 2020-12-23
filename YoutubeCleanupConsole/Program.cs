using Autofac;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using YoutubeCleanupTool;
using YoutubeCleanupTool.DataAccess;
using YoutubeCleanupTool.Domain;

namespace YoutubeCleanupConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<YouTubeCleanupToolModule>();
            builder.RegisterModule<YoutubeCleanupConsoleModule>();
            builder.RegisterModule<YouTubeCleanupToolDefaultModule>();

            var dbContextBuilder = new DbContextOptionsBuilder<YoutubeCleanupToolDbContext>();
            dbContextBuilder.UseSqlite("Data Source=Application.db");
            builder.RegisterInstance(dbContextBuilder.Options).As<Microsoft.EntityFrameworkCore.DbContextOptions>();
            builder.RegisterType<YoutubeCleanupToolDbContext>().As<IYouTubeCleanupToolDbContext>();
            var container = builder.Build();
            try
            {
                container.Resolve<IYouTubeCleanupToolDbContext>().Migrate();
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
