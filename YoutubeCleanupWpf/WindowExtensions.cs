using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;

namespace YouTubeCleanupWpf
{
    public class WindowExtensions
    {
        private readonly DoWorkOnUi _doWorkOnUi;
        public WindowExtensions([NotNull] DoWorkOnUi doWorkOnUi) => _doWorkOnUi = doWorkOnUi;

        public async Task StartOnSelectedWindow(Window window, WpfSettings wpfSettings)
        {
            await _doWorkOnUi.RunOnUiThreadAsync(() =>
            {
                window.Top = wpfSettings.CurrentScreen.Bounds.Top + 100;
                window.Left = wpfSettings.CurrentScreen.Bounds.Left + 100;
            });
        }
    }
}
