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
            builder.RegisterType<ConsoleUi>().As<IConsoleUi>();
            builder.RegisterType<YouTubeServiceCreator>().As<IYouTubeServiceCreator>();
            builder.RegisterType<CredentialManagerWrapper>().As<ICredentialManagerWrapper>();
            builder.RegisterType<Persister>().As<IPersister>();
            builder.RegisterInstance(new YoutubeServiceCreatorOptions
            {
                ClientSecretPath = @"C:\Users\unopl\source\repos\Creds\client_secret.json",
                FileDataStoreName = "Youtube.Api.Storage",
            });
            builder.Register(x =>
            {
                var creator = x.Resolve<IYouTubeServiceCreator>();
                return creator.CreateYouTubeService().Result;
            }).As<IYouTubeServiceWrapper>();
        }
    }
}
