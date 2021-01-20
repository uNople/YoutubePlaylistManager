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
                // TODO: this seems flaky - if we don't initialize WpfSettings first, then the default DB settings don't get changed
                // We probably actually need to call .Migrate when calling Create()?
                container.Resolve<WpfSettings>(); // just so the ctor gets called and the settings get loaded
                container.Resolve<IYouTubeCleanupToolDbContextFactory>().Create().Migrate();
                MainWindow = container.Resolve<MainWindow>();
                MainWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unexpected error: {ex}");
                throw;
            }
        }
    }
}
