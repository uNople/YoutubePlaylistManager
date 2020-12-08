using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class YouTubeCleanupToolModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<YouTubeServiceCreator>().As<IYouTubeServiceCreator>();
            builder.RegisterType<CredentialManagerWrapper>().As<ICredentialManagerWrapper>();
            builder.RegisterType<Persister>().As<IPersister>();
            builder.RegisterInstance(new YoutubeServiceCreatorOptions
            {
                // TODO: Move to app settings
                ClientSecretPath = @"C:\temp\client_secret.json",
                FileDataStoreName = "Youtube.Api.Storage",
            });
            builder.RegisterType<WhereTheRubberHitsTheRoad>().As<IWhereTheRubberHitsTheRoad>();
        }
    }
}
