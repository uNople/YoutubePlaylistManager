using System.Diagnostics.CodeAnalysis;
using System.Windows;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, ISettingsWindow
    {
        private readonly SettingsWindowViewModel _settingsWindowViewModel;
        private readonly WpfSettings _wpfSettings;

        public SettingsWindow([NotNull] SettingsWindowViewModel settingsWindowViewModel, [NotNull] WpfSettings wpfSettings)
        {
            _settingsWindowViewModel = settingsWindowViewModel;
            DataContext = _settingsWindowViewModel;
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

    public interface ISettingsWindow
    {
        void Show();
    }
}
