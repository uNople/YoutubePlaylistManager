using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using YouTubeCleanupWpf.Converters;

namespace YouTubeCleanupWpf.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
#pragma warning disable 067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
        public WpfSettings WpfSettings { get; set; }
        public ICommand SelectDbPathCommand { get; set; }
        public ICommand SelectClientSecretPathCommand { get; set; }
        public ICommand OpenDeveloperConsoleCommand { get; set; }


        public SettingsWindowViewModel([NotNull] WpfSettings wpfSettings, [NotNull] IErrorHandler errorHandler)
        {
            WpfSettings = wpfSettings;
            SelectDbPathCommand = new RunMethodWithoutParameterCommand(SelectDbPath, errorHandler.HandleError);
            SelectClientSecretPathCommand = new RunMethodWithoutParameterCommand(SelectClientSecret, errorHandler.HandleError);
            OpenDeveloperConsoleCommand = new RunMethodWithoutParameterCommand(OpenDeveloperConsole, errorHandler.HandleError);
        }
        
        private async Task OpenDeveloperConsole()
        {
            await Task.Run(() => MainWindowViewModel.OpenLink("https://console.developers.google.com/?pli=1"));
        }

        private Task SelectClientSecret()
        {
            if (GetFileDialog(WpfSettings.ClientSecretPath, out string newPath))
            {
                WpfSettings.ClientSecretPath = newPath;
            }

            return Task.CompletedTask;
        }
        
        private Task SelectDbPath()
        {
            if (GetFileDialog(WpfSettings.DatabasePath, out var newPath))
            {
                WpfSettings.DatabasePath = newPath;
            }

            return Task.CompletedTask;
        }

        private bool GetFileDialog(string currentPath, out string newPath)
        {
            var initialDirectory = @"C:\";
            try
            {
                var path = Path.GetDirectoryName(currentPath);
                if (path != null)
                    initialDirectory = path;
            }
            catch
            {
                // don't care - if the above errors we have a value already for this
            }
            
            var dialog = new OpenFileDialog {FileName = Path.GetFileName(currentPath), InitialDirectory = initialDirectory};
            var result = dialog.ShowDialog();
            newPath = dialog.FileName;
            return result ?? false;
        }
    }
}
