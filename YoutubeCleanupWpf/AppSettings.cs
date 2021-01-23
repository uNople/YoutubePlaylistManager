using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class AppSettings : IAppSettings
    {
        public string ApiKey { get; set; }
    }
}
