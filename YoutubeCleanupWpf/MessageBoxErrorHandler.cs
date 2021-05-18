using System;
using System.Windows;
using YouTubeCleanup.Ui;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf
{
    public class MessageBoxErrorHandler : IErrorHandler
    {
        private readonly ILogger _logger;

        public MessageBoxErrorHandler(ILogger logger)
        {
            _logger = logger;
        }
        public void HandleError(Exception ex)
        {
            MessageBox.Show(ex.ToString());
            _logger.Error(ex.ToString());
        }
    }
}
