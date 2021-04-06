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


        public void ShowSettingsWindow()
        {
            _settingsWindow.Show();
        }

        public async Task ShowUpdateDataWindow(string title)
        {
            await _updateDataWindow.Show(title);
        }

        public void SetUpdateComplete()
        {
            _updateDataWindow.SetProgressBarState(ProgressBarState.Stopped);
        }
    }
}
