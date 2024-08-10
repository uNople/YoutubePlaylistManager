using System;
using System.Windows;
using YouTubeCleanup.Ui;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanupWpf;

public class MessageBoxErrorHandler(ILogger logger) : IErrorHandler
{
    public void HandleError(Exception ex)
    {
            MessageBox.Show(ex.ToString());
            logger.Error(ex.ToString());
        }
}