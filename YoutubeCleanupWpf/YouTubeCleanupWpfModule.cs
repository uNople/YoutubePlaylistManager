using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;

namespace YoutubeCleanupWpf
{
    public class YouTubeCleanupWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>();
            builder.RegisterType<MainWindowViewModel>();
            builder.RegisterAutoMapper(typeof(YouTubeCleanupWpfModule).Assembly);
        }
    }
}
