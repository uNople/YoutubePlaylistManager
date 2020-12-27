using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace YoutubeCleanupWpf
{
    public class YouTubeCleanupWpfModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainWindow>();
            builder.RegisterType<MainWindowViewModel>();
        }
    }
}
