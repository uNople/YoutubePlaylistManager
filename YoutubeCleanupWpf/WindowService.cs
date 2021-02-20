using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf
{
    public class WindowService : IWindowService
    {
        private readonly ISettingsWindow _settingsWindow;
        private readonly IUpdateDataWindow _updateDataWindow;

        public WindowService([NotNull] ISettingsWindow settingsWindow,
            [NotNull] IUpdateDataWindow updateDataWindow)
        {
            _settingsWindow = settingsWindow;
            _updateDataWindow = updateDataWindow;
        }


        public async Task ShowSettingsWindow()
        {
            await _settingsWindow.ShowWindow();
        }

        public async Task ShowUpdateDataWindow()
        {
            await _updateDataWindow.ShowWindow();
        }
    }
}
