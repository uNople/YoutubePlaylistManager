using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using YouTubeApiWrapper.Interfaces;
using YouTubeCleanupTool.Domain;

namespace YouTubeApiWrapper
{
    public class YouTubeApiWrapperModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(new YouTubeServiceCreatorOptions
            {
                DatabasePath = "Application.db",
                ClientSecretPath = @"C:\temp\client_secret.json",
                FileDataStoreName = "YouTube.Api.Storage",
            }).SingleInstance();
            builder.RegisterAutoMapper(typeof(YouTubeApiWrapperModule).Assembly);
            builder.RegisterType<YouTubeApi>().As<IYouTubeApi>();
        }
    }
}
