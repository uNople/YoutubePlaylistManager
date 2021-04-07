using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
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
        private readonly IAppClosingCancellationToken _appClosingCancellationToken;
#pragma warning disable 067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
        public string LogText { get; set; }
        public ICommand CloseCommand { get; set; }
        public UpdateDataWindow ParentWindow { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }
        private ConcurrentQueue<string> UiLogs { get; } = new();
        private ConcurrentQueue<string> DiskLogs { get; } = new();
        private Thread _writeLogsToUiThread;
        private Thread _writeLogsToDiskThread;
        private readonly StringBuilder _logStringBuilder = new StringBuilder();
        public string CurrentTitle { get; set; }
        public UpdateDataViewModel([NotNull]IErrorHandler errorHandler, [NotNull]ILogger<UpdateDataViewModel> logger, [NotNull]IAppClosingCancellationToken appClosingCancellationToken)
        {
            _logger = logger;
            _appClosingCancellationToken = appClosingCancellationToken;
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, errorHandler.HandleError);
            _writeLogsToUiThread = new Thread(WriteLogsToUi);
            _writeLogsToUiThread.Start();
            _writeLogsToDiskThread = new Thread(WriteLogsToDisk);
            _writeLogsToDiskThread.Start();
        }

        private void WriteLogsToDisk()
        {
            Thread.CurrentThread.Name = "Update logs to disk thread";
            const string logFile = "Log.txt";
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), logFile);
            while (true)
            {
                if (_appClosingCancellationToken.CancellationTokenSource.IsCancellationRequested)
                {
                    _writeLogsToDiskThread.Interrupt();
                    _writeLogsToDiskThread = null;
                    return;
                }

                var messages = new List<string>();
                while (DiskLogs.TryDequeue(out var message))
                {
                    messages.Add(message);
                }
                
                File.AppendAllLines(path, messages);
            }
        }

        private void WriteLogsToUi()
        {
            const int MAX_STRING_LENGTH = 10000;
            Thread.CurrentThread.Name = "Update logs in ui thread";
            while (true)
            {
                if (_appClosingCancellationToken.CancellationTokenSource.IsCancellationRequested)
                {
                    _logStringBuilder.Clear();
                    _writeLogsToUiThread.Interrupt();
                    _writeLogsToUiThread = null;
                    return;
                }
                
                var shouldAppend = false;
                while (UiLogs.TryDequeue(out var message))
                {
                    _logger.LogTrace(message);
                    _logStringBuilder.Insert(0, message + Environment.NewLine);
                    shouldAppend = true;
                }

                if (shouldAppend)
                {
                    // Clamp the string builder to 10,000 characters (better than creating yet another string to do the truncate on)
                    if (_logStringBuilder.Length > MAX_STRING_LENGTH)
                        _logStringBuilder.Remove(MAX_STRING_LENGTH, _logStringBuilder.Length - MAX_STRING_LENGTH);
                    
                    // NOTE: we still need to clamp the text's length to get a responsive UI
                    var logText = _logStringBuilder.ToString();
                    new Action(() => LogText = logText).RunOnUiThreadSync();
                }

                Thread.Sleep(100);
            }
        }

        public async Task PrependText(string message)
        {
            await Task.Run(() =>
            {
                UiLogs.Enqueue(message);
                DiskLogs.Enqueue($"{DateTime.Now:o} {message}");
            });
        }
        
        public Task Hide()
        {
            ParentWindow.Hide();

            // Lots of strings created + appended here means a lot on the heap. Closing takes  
            // a little time anyway, so doing a collect here won't cause any performance issues. 
            GC.Collect();
            return Task.CompletedTask;
        }
    }

    public interface IUpdateDataViewModel
    {
        Task PrependText(string message);
    }
}
