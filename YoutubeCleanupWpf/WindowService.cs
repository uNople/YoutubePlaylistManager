using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;

namespace YouTubeCleanupWpf
{
    public class WindowService
    {
        private readonly IContainer _container;

        public WindowService(IContainer container)
        {
            _container = container;
        }

    }
}
