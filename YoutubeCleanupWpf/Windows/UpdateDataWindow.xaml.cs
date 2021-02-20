using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.Windows
{
    /// <summary>
    /// Interaction logic for UpdateDataWindow.xaml
    /// </summary>
    public partial class UpdateDataWindow : IUpdateDataWindow
    {
        private readonly WpfSettings _wpfSettings;

        public UpdateDataWindow([NotNull] UpdateDataViewModel updateDataViewModel, [NotNull] WpfSettings wpfSettings)
        {
            updateDataViewModel.ParentWindow = this;
            DataContext = updateDataViewModel;
            _wpfSettings = wpfSettings;
            _ =  this.StartOnSelectedWindow(_wpfSettings);
            InitializeComponent();
        }
        
        public async Task ShowWindow()
        {
            await this.StartOnSelectedWindow(_wpfSettings);
            Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }

    public interface IUpdateDataWindow
    {
        Task ShowWindow();
    }
}
