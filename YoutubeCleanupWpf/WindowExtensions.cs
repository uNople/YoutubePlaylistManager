using System;
using System.Threading.Tasks;
using System.Windows;

namespace YouTubeCleanupWpf
{
    public static class WindowExtensions
    {
        public static async Task StartOnSelectedWindow(this Window window, WpfSettings wpfSettings)
        {
            await new Action(() =>
            {
                window.Top = wpfSettings.CurrentScreen.Bounds.Top + 100;
                window.Left = wpfSettings.CurrentScreen.Bounds.Left + 100;
            }).RunOnUiThread();
        }
    }
}
