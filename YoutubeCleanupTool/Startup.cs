using Autofac;
using System;
using System.Collections.Generic;
using System.Text;
using YoutubeCleanupTool.Interfaces;

namespace YoutubeCleanupTool
{
    public class Startup
    {
        internal void Run()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<YouTubeCleanupToolModule>();
            var container = builder.Build();
            container.Resolve<IConsoleUi>().Run();
        }
    }
}
