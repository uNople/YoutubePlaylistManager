using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using YouTubeCleanupWpf.Converters;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf.ViewModels
{
    public class UpdateDataViewModel : INotifyPropertyChanged, IUpdateDataViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string LogText { get; set; }
        public ICommand CloseCommand { get; set; }
        public UpdateDataWindow ParentWindow { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }
        private ConcurrentQueue<string> PendingLogs { get; } = new ConcurrentQueue<string>();
        private readonly Timer _renderLogsToUi;

        public UpdateDataViewModel()
        {
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, MainWindowViewModel.ShowError);
            _renderLogsToUi = new Timer(DequeueLogs, null, TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(200));
        }

        private void DequeueLogs(object? state)
        {
            var logMessage = "";
            while (PendingLogs.TryDequeue(out var message))
            {
                logMessage = string.IsNullOrWhiteSpace(logMessage) ? message : $"{message}{Environment.NewLine}{logMessage}";
            }

            if (!string.IsNullOrWhiteSpace(logMessage))
            {
                new Action(() => LogText = $"{logMessage}{Environment.NewLine}{LogText}").RunOnUiThread();
            }
        }

        public void PrependText(string message)
        {
            PendingLogs.Enqueue(message);
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

    public interface IUpdateDataViewModel
    {
        CancellationTokenSource CancellationTokenSource { get; set; }
        MainWindowViewModel MainWindowViewModel { get; set; }
        void PrependText(string message);
    }
}
