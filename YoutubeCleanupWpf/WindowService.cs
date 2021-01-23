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
