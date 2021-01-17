using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Microsoft.EntityFrameworkCore;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupTool.DataAccess
{
    public class YouTubeCleanupToolDataModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<YouTubeCleanupToolDbContextFactory>().As<IYouTubeCleanupToolDbContextFactory>();
        }
    }
}
