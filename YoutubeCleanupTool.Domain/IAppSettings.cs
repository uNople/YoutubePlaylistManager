using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YouTubeCleanupTool.Domain
{
    public interface IAppSettings
    {
        string ApiKey { get; set; }
    }
}
