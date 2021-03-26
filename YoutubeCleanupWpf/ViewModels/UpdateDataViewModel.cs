using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using YouTubeCleanupWpf.Converters;
using YouTubeCleanupWpf.Windows;

namespace YouTubeCleanupWpf.ViewModels
{
    public class UpdateDataViewModel : INotifyPropertyChanged, IUpdateDataViewModel
    {
        private readonly ILogger<UpdateDataViewModel> _logger;
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
        private readonly StringBuilder _logStringBuilder = new StringBuilder();

        public UpdateDataViewModel([NotNull]IErrorHandler errorHandler, [NotNull]ILogger<UpdateDataViewModel> logger)
        {
            _logger = logger;
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, errorHandler.HandleError);
        }

        internal async Task Start()
        {
            await new Action(() => LogText = "").RunOnUiThreadAsync();
            
            if (_currentThread == null)
            {
                _currentThread = new Thread(DequeueLogs);
                _currentThread.Start();
            }
        }

        private void DequeueLogs()
        {
            Thread.CurrentThread.Name = "Update logs thread";
            while (true)
            {
                if ((CancellationTokenSource?.IsCancellationRequested ?? false) && PendingLogs.IsEmpty)
                {
                    _logStringBuilder.Clear();
                    _currentThread.Interrupt();
                    _currentThread = null;
                    return;
                }
                
                var shouldAppend = false;
                while (PendingLogs.TryDequeue(out var message))
                {
                    _logger.LogTrace(message);
                    _logStringBuilder.Insert(0, message + Environment.NewLine);
                    shouldAppend = true;
                }

                if (shouldAppend)
                {
                    // Clamp the string builder to 10,000 characters (better than creating yet another string to do the truncate on)
                    // TODO: Implement
                    
                    // NOTE: we still need to clamp the text's length to get a responsive UI
                    var logText = _logStringBuilder.ToString();
                    logText = logText.Substring(0, Math.Min(logText.Length, 10000));
                    new Action(() => LogText = logText).RunOnUiThreadSync();
                }

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
            if (MainWindowViewModel != null)
            {
                MainWindowViewModel.UpdateHappening = false;
            }

            // Lots of strings created + appended here means a lot on the heap. Closing takes  
            // a little time anyway, so doing a collect here won't cause any performance issues. 
            GC.Collect();
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
