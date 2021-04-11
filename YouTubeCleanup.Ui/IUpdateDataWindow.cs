using System.Threading.Tasks;

namespace YouTubeCleanup.Ui
{
    public interface IUpdateDataWindow
    {
        Task Show(string title);
        void Hide();
    }
}