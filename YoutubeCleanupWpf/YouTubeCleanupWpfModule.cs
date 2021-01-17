using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using YouTubeCleanupTool.Domain;
using YouTubeCleanupWpf;

namespace YoutubeCleanupWpf
{
    public class YouTubeCleanupWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>();
            builder.RegisterType<UpdateDataWindow>();
            builder.RegisterType<SettingsWindow>();
            builder.RegisterType<MainWindowViewModel>().SingleInstance();
            builder.RegisterType<UpdateDataViewModel>().SingleInstance();
            builder.RegisterType<SettingsWindowViewModel>().SingleInstance();
            builder.RegisterAutoMapper(typeof(YouTubeCleanupWpfModule).Assembly);

            builder.Register(x =>
                {
                    var youTubeServiceCreatorOptions = x.Resolve<YouTubeServiceCreatorOptions>();
                    try
                    {
                        var wpfSettings = JsonConvert.DeserializeObject<WpfSettings>(File.ReadAllText("WpfSettings.json"));
                        wpfSettings.YouTubeServiceCreatorOptions = youTubeServiceCreatorOptions;
                        return wpfSettings;
                    }
                    catch
                    {
                        return new WpfSettings(youTubeServiceCreatorOptions);
                    }
                    
                })
                .OnActivating(x => x.Instance.InitializeSettings())
                .SingleInstance();
        }
    }
}
