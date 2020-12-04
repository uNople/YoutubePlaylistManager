using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeCleanupTool
{
    public class YouTubeCleanupToolModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleUi>().As<IConsoleUi>();
            builder.RegisterType<YouTubeServiceCreator>().As<IYouTubeServiceCreator>();
            builder.RegisterType<CredentialManagerWrapper>().As<ICredentialManagerWrapper>();
        }
    }
}
