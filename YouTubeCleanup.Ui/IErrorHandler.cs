using System;

namespace YouTubeCleanup.Ui;

public interface IErrorHandler
{
    void HandleError(Exception ex);
}