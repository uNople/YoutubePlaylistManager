using Autofac;
using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using YouTubeApiWrapper;
using YouTubeCleanupTool.DataAccess;
using YouTubeCleanupTool.Domain;
using Autofac.Configuration;
using YouTubeCleanupWpf;

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
            builder.RegisterModule<YouTubeCleanupToolDataModule>();
            var container = builder.Build();
            try
            {
                container.Resolve<IYouTubeCleanupToolDbContextFactory>().Create().Migrate();
                container.Resolve<WpfSettings>(); // just so the ctor gets called and the settings get loaded
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
