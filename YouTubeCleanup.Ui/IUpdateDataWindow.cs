using System.Threading.Tasks;

namespace YouTubeCleanupWpf.Windows
{
    public interface IUpdateDataWindow
    {
        Task Show(string title);
        void Hide();
    }
}