namespace YouTubeCleanupWpf.Windows;

public interface IPasswordPrompt
{
    byte[] GetEntropy();
    bool? ShowDialog();
}