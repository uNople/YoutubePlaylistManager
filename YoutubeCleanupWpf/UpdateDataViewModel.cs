using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YoutubeCleanupWpf;

namespace YouTubeCleanupWpf
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
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, ShowError);
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
        
        private static void ShowError(Exception ex) => MessageBox.Show(ex.ToString());
    }
}
