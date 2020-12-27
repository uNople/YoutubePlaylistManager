using Autofac;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using YouTubeApiWrapper;
using YouTubeCleanupTool.DataAccess;
using YouTubeCleanupTool.Domain;

namespace YoutubeCleanupWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<YouTubeApiWrapperModule>();
            builder.RegisterModule<YouTubeCleanupToolDefaultModule>();
            builder.RegisterModule<YouTubeCleanupWpfModule>();

            var dbContextBuilder = new DbContextOptionsBuilder<YoutubeCleanupToolDbContext>();
            dbContextBuilder.UseSqlite(@"Data Source=D:\uNople\Programming\YoutubeCleanupTool\YoutubeCleanupConsole\bin\Debug\net5.0\Application.db");
            builder.RegisterInstance(dbContextBuilder.Options).As<DbContextOptions>();
            builder.RegisterType<YoutubeCleanupToolDbContext>().As<IYouTubeCleanupToolDbContext>();
            var container = builder.Build();
            try
            {
                container.Resolve<IYouTubeCleanupToolDbContext>().Migrate();
                container.Resolve<MainWindow>().Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex}");
                throw;
            }
        }
    }
}
