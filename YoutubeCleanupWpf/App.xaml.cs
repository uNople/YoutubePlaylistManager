using System;
using System.Windows;
using Autofac;
using YouTubeApiWrapper;
using YouTubeCleanupTool.DataAccess;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
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
                MainWindow?.Show();
            }
            catch (Exception ex)
            {
                var message = $"Unexpected error: {ex}";
                MessageBox.Show(message);
                container.Resolve<ILogger>().Fatal(message);
                throw;
            }
        }
    }
}
