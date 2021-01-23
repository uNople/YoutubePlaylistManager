using System.Windows;

namespace YouTubeCleanupWpf
{
    public static class WindowExtensions
    {
        public static void StartOnSelectedWindow(this Window window, WpfSettings wpfSettings)
        {
            window.Top = wpfSettings.CurrentScreen.Bounds.Top + 100;
            window.Left = wpfSettings.CurrentScreen.Bounds.Left + 100;
        }
    }
}
