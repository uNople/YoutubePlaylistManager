using System;
using System.Diagnostics.CodeAnalysis;
using YouTubeCleanupWpf.ViewModels;

namespace YouTubeCleanupWpf.Windows
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : ISettingsWindow
    {
        private readonly WpfSettings _wpfSettings;

        public SettingsWindow([NotNull] SettingsWindowViewModel settingsWindowViewModel, [NotNull] WpfSettings wpfSettings)
        {
            DataContext = settingsWindowViewModel;
            _wpfSettings = wpfSettings;
            this.StartOnSelectedWindow(_wpfSettings);
            InitializeComponent();
        }

        public new void Show()
        {
            this.StartOnSelectedWindow(_wpfSettings);
            new Action(() => base.Show()).RunOnUiThread();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            new Action(Hide).RunOnUiThread();
        }
    }

    public interface ISettingsWindow
    {
        void Show();
    }
}
