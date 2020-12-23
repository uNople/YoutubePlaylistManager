using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeCleanupTool.Domain
{
    public class YouTubeCleanupToolDefaultModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<GetAndCacheYouTubeData>().As<IGetAndCacheYouTubeData>();
        }
    }
}
