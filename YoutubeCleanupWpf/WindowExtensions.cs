using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows;
using YouTubeCleanup.Ui;

namespace YouTubeCleanupWpf;

public class WindowExtensions([NotNull] IDoWorkOnUi doWorkOnUi)
{
    public async Task StartOnSelectedWindow(Window window, WpfSettings wpfSettings)
    {
        await doWorkOnUi.RunOnUiThreadAsync(() =>
        {
            window.Top = wpfSettings.CurrentScreen.Bounds.Top + 100;
            window.Left = wpfSettings.CurrentScreen.Bounds.Left + 100;
        });
    }
}