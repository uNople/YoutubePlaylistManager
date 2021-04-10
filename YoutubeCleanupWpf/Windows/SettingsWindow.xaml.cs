using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : ISettingsWindow
    {
        private readonly WpfSettings _wpfSettings;
        private readonly WindowExtensions _windowExtensions;
        private readonly DoWorkOnUi _doWorkOnUi;

        public SettingsWindow([NotNull] SettingsWindowViewModel settingsWindowViewModel,
            [NotNull] WpfSettings wpfSettings,
            [NotNull] WindowExtensions windowExtensions,
            [NotNull] DoWorkOnUi doWorkOnUi)
        {
            DataContext = settingsWindowViewModel;
            _wpfSettings = wpfSettings;
            _windowExtensions = windowExtensions;
            _doWorkOnUi = doWorkOnUi;
            Task.Run(async () => await _windowExtensions.StartOnSelectedWindow(this, _wpfSettings));
            InitializeComponent();
        }

        public new async Task Show()
        {
            await _windowExtensions.StartOnSelectedWindow(this, _wpfSettings);
            await _doWorkOnUi.RunOnUiThreadAsync(() => base.Show());
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await _doWorkOnUi.RunOnUiThreadAsync(Hide);
        }
    }

    public interface ISettingsWindow
    {
        Task Show();
    }
}
