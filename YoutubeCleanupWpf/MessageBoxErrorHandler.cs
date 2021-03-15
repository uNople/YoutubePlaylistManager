using System;
using System.Windows;

namespace YouTubeCleanupWpf
{
    public class MessageBoxErrorHandler : IErrorHandler
    {
        public void HandleError(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
