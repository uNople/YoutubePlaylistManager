using Autofac;
using AutoMapper.Contrib.Autofac.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.Domain;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class YouTubeCleanupToolModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<CredentialManagerWrapper>().As<ICredentialManagerWrapper>();
            builder.RegisterInstance(new YoutubeServiceCreatorOptions
            {
                // TODO: Move to app settings
                ClientSecretPath = @"C:\temp\client_secret.json",
                FileDataStoreName = "Youtube.Api.Storage",
            });
            builder.RegisterAutoMapper(typeof(YouTubeCleanupToolModule).Assembly);
            builder.RegisterType<YouTubeApi>().As<IYouTubeApi>();
        }
    }
}
