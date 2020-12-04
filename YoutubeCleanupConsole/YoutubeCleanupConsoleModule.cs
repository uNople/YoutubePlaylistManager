using Autofac;
using System;
using System.Collections.Generic;
using System.Text;

namespace YoutubeCleanupConsole
{
    public class YoutubeCleanupConsoleModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ConsoleUi>().As<IConsoleUi>();
            builder.RegisterType<ConsoleDisplayParams>();
        }
    }
}
