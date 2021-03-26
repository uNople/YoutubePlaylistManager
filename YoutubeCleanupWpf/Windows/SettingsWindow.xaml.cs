using System;
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

        public SettingsWindow([NotNull] SettingsWindowViewModel settingsWindowViewModel, [NotNull] WpfSettings wpfSettings)
        {
            DataContext = settingsWindowViewModel;
            _wpfSettings = wpfSettings;
            Task.Run(async () => await this.StartOnSelectedWindow(_wpfSettings));
            InitializeComponent();
        }

        public new async Task Show()
        {
            await this.StartOnSelectedWindow(_wpfSettings);
            await new Action(() => base.Show()).RunOnUiThreadAsync();
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            await new Action(Hide).RunOnUiThreadAsync();
        }
    }

    public interface ISettingsWindow
    {
        Task Show();
    }
}
