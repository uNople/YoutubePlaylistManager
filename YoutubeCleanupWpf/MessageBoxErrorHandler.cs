using System;
using System.Windows;
using YouTubeCleanup.Ui;

namespace YouTubeCleanupWpf
{
    public class MessageBoxErrorHandler : IErrorHandler
    {
        public void HandleError(Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }
}
