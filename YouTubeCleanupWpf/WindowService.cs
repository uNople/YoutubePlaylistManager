using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YouTubeCleanup.Ui;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf;

public class WindowService(
    [NotNull] ISettingsWindow settingsWindow,
    [NotNull] IUpdateDataWindow updateDataWindow,
    [NotNull] IPasswordPrompt passwordPrompt)
    : IWindowService
{
    public void ShowSettingsWindow()
    {
        settingsWindow.Show();
    }

    public async Task ShowUpdateDataWindow(string title)
    {
        await updateDataWindow.Show(title);
    }

    public Task<byte[]> PromptForEntropy()
    {
        if (passwordPrompt.ShowDialog() ?? false)
        {
            return Task.FromResult(passwordPrompt.GetEntropy());
        }

        return null;
    }
}