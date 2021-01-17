using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
