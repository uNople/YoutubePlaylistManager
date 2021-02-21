using System;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf.ViewModels;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf
{
    public class YouTubeCleanupWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>();
            builder.RegisterType<UpdateDataWindow>().As<IUpdateDataWindow>();
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

            builder.Register(x =>
                {
                    var youTubeServiceCreatorOptions = x.Resolve<YouTubeServiceCreatorOptions>();
                    var errorHandler = x.Resolve<IErrorHandler>();
                    try
                    {
                        var wpfSettings = JsonConvert.DeserializeObject<WpfSettings>(File.ReadAllText("WpfSettings.json"));
                        wpfSettings.YouTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
                        wpfSettings.ErrorHandler = errorHandler;
                        return wpfSettings;
                    }
                    catch
                    {
                        return new WpfSettings(youTubeServiceCreatorOptions, errorHandler);
                    }
                    
                })
                .OnActivating(x => x.Instance.InitializeSettings())
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
}
