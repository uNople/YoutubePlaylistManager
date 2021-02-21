using System.Diagnostics.CodeAnalysis;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.Windows
{
    /// <summary>
    /// Interaction logic for UpdateDataWindow.xaml
    /// </summary>
    public partial class UpdateDataWindow : IUpdateDataWindow
    {
        private readonly UpdateDataViewModel _updateDataViewModel;
        private readonly WpfSettings _wpfSettings;

        public UpdateDataWindow([NotNull] UpdateDataViewModel updateDataViewModel, [NotNull] WpfSettings wpfSettings)
        {
            _updateDataViewModel = updateDataViewModel;
            _updateDataViewModel.ParentWindow = this;
            DataContext = _updateDataViewModel;
            _wpfSettings = wpfSettings;
            this.StartOnSelectedWindow(_wpfSettings);
            InitializeComponent();
        }
        
        public new void Show()
        {
            this.StartOnSelectedWindow(_wpfSettings);
            SetProgressBarState(ProgressBarState.Running);
            _updateDataViewModel.Start();
            base.Show();
        }

        public void SetProgressBarState(ProgressBarState state)
        {
            ProgressBar.IsIndeterminate = state == ProgressBarState.Running;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            _updateDataViewModel.Hide();
            Hide();
        }
    }

    public interface IUpdateDataWindow
    {
        void Show();
        void SetProgressBarState(ProgressBarState state);
    }

    public enum ProgressBarState
    {
        Running,
        Stopped
    }
}
