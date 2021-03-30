using System;
using System.IO;
using System.Reflection;
using System.Windows;
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
using Module = Autofac.Module;

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
                    const string settingsFileName = "WpfSettings.json";
                    try
                    {
                        // TODO: Sometimes... the settings disappear. Should figure out why and fix it
                        var wpfSettings = JsonConvert.DeserializeObject<WpfSettings>(File.ReadAllText(settingsFileName));
                        wpfSettings.YouTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
                        wpfSettings.ErrorHandler = errorHandler;
                        return wpfSettings;
                    }
                    catch (Exception ex)
                    {
                        var currentDirectory = Environment.CurrentDirectory;
                        var assemblyDirectory = Assembly.GetExecutingAssembly().Location;
                        var logMessage = @$"Current dir: {currentDirectory}
Assembly dir: {assemblyDirectory}
File exists in whatever dir: {File.Exists(settingsFileName)}
File exists in current dir: {File.Exists(Path.Combine(currentDirectory, settingsFileName))}
File exists in assembly dir: {File.Exists(Path.Combine(assemblyDirectory, settingsFileName))}
Error: {ex}";
                        MessageBox.Show(logMessage);
                        throw new InvalidOperationException();
                    }
                })
                .OnActivating(x => x?.Instance?.InitializeSettings())
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
