using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using YouTubeCleanup.Ui;

namespace YouTubeCleanupWpf
{
    public class WindowExtensions
    {
        private readonly IDoWorkOnUi _doWorkOnUi;
        public WindowExtensions([NotNull] IDoWorkOnUi doWorkOnUi) => _doWorkOnUi = doWorkOnUi;

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
