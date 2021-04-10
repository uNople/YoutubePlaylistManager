using System;

namespace YouTubeCleanupWpf
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}