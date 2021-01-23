using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using YouTubeCleanupWpf.Converters;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf.ViewModels
{
    public class UpdateDataViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string LogText { get; set; }
        public ICommand CloseCommand { get; set; }
        public UpdateDataWindow ParentWindow { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; internal set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }

        public UpdateDataViewModel()
        {
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, MainWindowViewModel.ShowError);
        }
        
        public void PrependText(string message)
        {
            new Action(() => LogText = $"{message}{Environment.NewLine}{LogText}").RunOnUiThread();
        }

        private async Task Hide()
        {
            CancellationTokenSource?.Cancel();
            LogText = "";
            ParentWindow.Hide();
            await Task.Run(() => new Action(() => ParentWindow.Hide()).RunOnUiThread());
            MainWindowViewModel.UpdateHappening = false;
        }
    }
}
