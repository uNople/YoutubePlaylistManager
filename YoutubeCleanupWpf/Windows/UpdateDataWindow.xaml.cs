using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YouTubeCleanup.Ui;
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
        private readonly IDoWorkOnUi _doWorkOnUi;
        private readonly WindowExtensions _windowExtensions;

        public UpdateDataWindow([NotNull] UpdateDataViewModel updateDataViewModel, 
            [NotNull] WpfSettings wpfSettings,
            [NotNull] IDoWorkOnUi doWorkOnUi,
            [NotNull] WindowExtensions windowExtensions)
        {
            _updateDataViewModel = updateDataViewModel;
            _updateDataViewModel.ParentWindow = this;
            DataContext = _updateDataViewModel;
            _wpfSettings = wpfSettings;
            _doWorkOnUi = doWorkOnUi;
            _windowExtensions = windowExtensions;
            Task.Run(async () => await _windowExtensions.StartOnSelectedWindow(this, _wpfSettings));
            InitializeComponent();
        }

        public async Task Show(string title)
        {
            await _doWorkOnUi.RunOnUiThreadAsync(() => _updateDataViewModel.CurrentTitle = title);

            // show + bring to front
            if (!IsVisible)
            {
                await _windowExtensions.StartOnSelectedWindow(this, _wpfSettings);
                base.Show();
            }

            Activate();
            Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            _updateDataViewModel.Hide();
            Hide();
        }
    }
}