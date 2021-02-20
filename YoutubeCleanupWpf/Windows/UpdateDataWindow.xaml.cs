using System.Diagnostics.CodeAnalysis;
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
            this.StartOnSelectedWindow(_wpfSettings);
            InitializeComponent();
        }
        
        public new void Show()
        {
            this.StartOnSelectedWindow(_wpfSettings);
            base.Show();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }

    public interface IUpdateDataWindow
    {
        void Show();
    }
}
