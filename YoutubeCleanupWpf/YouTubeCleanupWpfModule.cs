using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeCleanup.Ui;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf.ViewModels;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf;

public class YouTubeCleanupWpfModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
            // NOTE: even though this isn't registered as SingleInstance, I think it ends up being SingleInstance anyway
            // Since we're starting App.xaml.cs, which only launches one MainWindow...
            builder.RegisterType<MainWindow>();
            // TODO: add an integration / e2e test which checks this.
            // The test needs to:
            // - Talk to window service
            // - show the update data window
            // - Call .hide() on the viewmodel
            // - Verify this instance of IUpdateDataWindow gets called
            // - Then re-do, and make sure the same instance gets called.
            // The underlying thing here is that when we show the window, it's a different instance
            // but we hold a reference to the first window (I think) in the viewmodel
            // potentially another way of validating this is to ensure that the window that gets shown when
            // talking to the window service is the same instance the second time around as the viewmodel knows about
            builder.RegisterType<UpdateDataWindow>().As<IUpdateDataWindow>().SingleInstance();
            builder.RegisterType<SettingsWindow>().As<ISettingsWindow>();
            builder.RegisterType<MainWindowViewModel>().SingleInstance();
            builder.RegisterType<MessageBoxErrorHandler>().As<IErrorHandler>();
            builder.RegisterType<UpdateDataViewModel>()
                .As<IUpdateDataViewModel>()
                .As<UpdateDataViewModel>()
                .SingleInstance();
            builder.RegisterType<SettingsWindowViewModel>().SingleInstance();
            builder.RegisterType<WindowService>().As<IWindowService>();
            builder.RegisterAutoMapper(typeof(YouTubeCleanupWpfModule).Assembly);
            builder.RegisterType<AppClosingCancellationToken>().As<IAppClosingCancellationToken>().SingleInstance();
            builder.RegisterType<DoWorkOnUi>().As<IDoWorkOnUi>();
            builder.RegisterType<WindowExtensions>();

            // Password prompt / settings encryption things
            builder.RegisterType<EntropyService>().As<IEntropyService>();
            builder.RegisterType<DpapiService>().As<IDpapiService>();
            builder.RegisterType<PasswordPrompt>().As<IPasswordPrompt>();

            builder.Register(x =>
                {
                    var youTubeServiceCreatorOptions = x.Resolve<YouTubeServiceCreatorOptions>();
                    var errorHandler = x.Resolve<IErrorHandler>();
                    try
                    {
                        var wpfSettings = JsonConvert.DeserializeObject<WpfSettings>(File.ReadAllText("WpfSettings.json"));
                        if (wpfSettings != null)
                        {
                            wpfSettings.YouTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
                            wpfSettings.ErrorHandler = errorHandler;
                        }
                        else
                        {
                            wpfSettings = new WpfSettings(youTubeServiceCreatorOptions, errorHandler);
                        }

                        return wpfSettings;
                    }
                    catch
                    {
                        return new WpfSettings(youTubeServiceCreatorOptions, errorHandler);
                    }
                })
                .OnActivating(x => x?.Instance?.InitializeSettings())
                .As<WpfSettings>()
                .As<IDebugSettings>()
                .SingleInstance();

            var config = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .AddUserSecrets<App>()
                .Build();
            
            var section = config.GetSection(nameof(AppSettings));
            var appSettingsConfig = section.Get<AppSettings>();
            builder.RegisterInstance(appSettingsConfig).As<IAppSettings>().SingleInstance();

            var services = new ServiceCollection();
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.SetMinimumLevel(LogLevel.Information);
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
            builder.Populate(services);
        }
}