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
#pragma warning disable 067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
        public string LogText { get; set; }
        public ICommand CloseCommand { get; set; }
        public UpdateDataWindow ParentWindow { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }
        private ConcurrentQueue<string> PendingLogs { get; } = new ConcurrentQueue<string>();
        private readonly Timer _timer;
        private readonly int _timerDuration;

        public UpdateDataViewModel()
        {
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, MainWindowViewModel.ShowError);
            _timerDuration = (int) TimeSpan.FromMilliseconds(1000).TotalMilliseconds;
            _timer = new Timer(DequeueLogs, null, _timerDuration, Timeout.Infinite);
        }

        private void DequeueLogs(object state)
        {
            try
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

                _timer.Change(_timerDuration, Timeout.Infinite);
            }
            catch (ObjectDisposedException)
            {
                // If we're in here, the timer's been disposed (probably the app exiting) while we're setting the next callback up
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
