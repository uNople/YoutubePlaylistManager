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
        private ConcurrentQueue<string> PendingLogs { get; } = new();
        private Thread _currentThread;

        public UpdateDataViewModel()
        {
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, MainWindowViewModel.ShowError);
        }

        internal void Start()
        {
            _currentThread = new Thread(DequeueLogs);
            _currentThread.Start();
        }

        private void DequeueLogs()
        {
            Thread.CurrentThread.Name = "Update logs thread";
            while (true)
            {
                var logMessage = "";
                while (PendingLogs.TryDequeue(out var message))
                {
                    logMessage = string.IsNullOrWhiteSpace(logMessage) ? message : $"{message}{Environment.NewLine}{logMessage}";
                }

                if (!string.IsNullOrWhiteSpace(logMessage))
                {
                    // NOTE: we still need to clamp the text's length to get a responsive UI
                    var logText = $"{logMessage}{Environment.NewLine}{LogText}";
                    logText = logText.Substring(0, Math.Min(logText.Length, 10000));
                    new Action(() => LogText = logText).RunOnUiThread();
                }

                if (CancellationTokenSource?.IsCancellationRequested ?? false)
                    return;

                Thread.Sleep(100);
            }
        }

        public void PrependText(string message)
        {
            PendingLogs.Enqueue(message);
        }

        public Task Hide()
        {
            CancellationTokenSource?.Cancel();
            LogText = "";
            ParentWindow.Hide();
            MainWindowViewModel.UpdateHappening = false;
            return Task.CompletedTask;
        }
    }

    public interface IUpdateDataViewModel
    {
        CancellationTokenSource CancellationTokenSource { get; set; }
        MainWindowViewModel MainWindowViewModel { get; set; }
        void PrependText(string message);
    }
}
