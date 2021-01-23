using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using YouTubeApiWrapper.Interfaces;
using YouTubeCleanupWpf.Converters;

namespace YouTubeCleanupWpf.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public WpfSettings WpfSettings { get; set; }
        public ICommand SelectDbPathCommand { get; set; }
        public ICommand SelectClientSecretPathCommand { get; set; }
        public ICommand UpdateApiKeyCommand { get; set; }
        public ICommand OpenDeveloperConsoleCommand { get; set; }


        public SettingsWindowViewModel([NotNull] WpfSettings wpfSettings)
        {
            WpfSettings = wpfSettings;
            SelectDbPathCommand = new RunMethodWithoutParameterCommand(SelectDbPath, MainWindowViewModel.ShowError);
            SelectClientSecretPathCommand = new RunMethodWithoutParameterCommand(SelectClientSecret, MainWindowViewModel.ShowError);
            UpdateApiKeyCommand = new RunMethodWithoutParameterCommand(UpdateApiKey, MainWindowViewModel.ShowError);
            OpenDeveloperConsoleCommand = new RunMethodWithoutParameterCommand(OpenDeveloperConsole, MainWindowViewModel.ShowError);

        }
        
        private async Task OpenDeveloperConsole()
        {
            MainWindowViewModel.OpenLink("https://console.developers.google.com/?pli=1");
        }
        
        private async Task UpdateApiKey()
        {
            // TODO: prompt to add secret somehow
        }

        private async Task SelectClientSecret()
        {
            if (GetFileDialog(WpfSettings.ClientSecretPath, out string newPath))
            {
                WpfSettings.ClientSecretPath = newPath;
            }
        }
        
        private async Task SelectDbPath()
        {
            if (GetFileDialog(WpfSettings.DatabasePath, out string newPath))
            {
                WpfSettings.DatabasePath = newPath;
            }
        }

        private bool GetFileDialog(string currentPath, out string newPath)
        {
            string initialDirectory = null;
            try
            {
                initialDirectory = Path.GetDirectoryName(currentPath);
            }
            catch
            {
            }

            initialDirectory ??= @"C:\";
            var dialog = new OpenFileDialog {FileName = System.IO.Path.GetFileName(currentPath), InitialDirectory = initialDirectory};
            var result = dialog.ShowDialog();
            newPath = dialog.FileName;
            return result ?? false;
        }
    }
}
