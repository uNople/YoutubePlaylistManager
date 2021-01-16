using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
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
            builder.RegisterType<UpdateDataWindow>().SingleInstance();
        }
    }
}
