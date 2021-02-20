using System.Threading.Tasks;

namespace YouTubeCleanupWpf
{
    public interface IWindowService
    {
        Task ShowSettingsWindow();
        Task ShowUpdateDataWindow();
    }
}