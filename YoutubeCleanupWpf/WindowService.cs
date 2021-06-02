using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YouTubeCleanup.Ui;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf
{
    public class WindowService : IWindowService
    {
        private readonly ISettingsWindow _settingsWindow;
        private readonly IUpdateDataWindow _updateDataWindow;
        private readonly IPasswordPrompt _passwordPrompt;

        public WindowService([NotNull] ISettingsWindow settingsWindow,
            [NotNull] IUpdateDataWindow updateDataWindow,
            [NotNull]IPasswordPrompt passwordPrompt)
        {
            _settingsWindow = settingsWindow;
            _updateDataWindow = updateDataWindow;
            _passwordPrompt = passwordPrompt;
        }


        public void ShowSettingsWindow()
        {
            _settingsWindow.Show();
        }

        public async Task ShowUpdateDataWindow(string title)
        {
            await _updateDataWindow.Show(title);
        }

        public Task<byte[]> PromptForEntropy()
        {
            if (_passwordPrompt.ShowDialog() ?? false)
            {
                return Task.FromResult(_passwordPrompt.GetEntropy());
            }
            return null;
        }
    }
}
