using System.Threading.Tasks;

namespace YouTubeCleanupWpf
{
    public interface IWindowService
    {
        void ShowSettingsWindow();
        Task ShowUpdateDataWindow();
        void SetUpdateComplete();
    }
}