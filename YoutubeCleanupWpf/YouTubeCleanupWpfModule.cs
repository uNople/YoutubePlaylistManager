using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using Microsoft.Extensions.Configuration;
using YouTubeCleanupWpf;

namespace YoutubeCleanupWpf
{
    public class YouTubeCleanupWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>();
            builder.RegisterType<MainWindowViewModel>();
            builder.RegisterAutoMapper(typeof(YouTubeCleanupWpfModule).Assembly);
            builder.RegisterType<UpdateDataViewModel>().SingleInstance();
            builder.RegisterType<UpdateDataWindow>();
            
            var config = new ConfigurationBuilder();
            config.AddJsonFile("appsettings.json");
            var builtConfig = config.Build();
            
            builder.Register(_ => builtConfig).As<IConfigurationRoot>();
            builder.RegisterType<WpfSettings>().SingleInstance();
        }
    }
}
