﻿using System;
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
        private readonly IAppClosingCancellationToken _appClosingCancellationToken;
#pragma warning disable 067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
        public string LogText { get; set; }
        public ICommand CloseCommand { get; set; }
        public UpdateDataWindow ParentWindow { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }
        private ConcurrentQueue<string> PendingLogs { get; } = new();
        private Thread _currentThread;
        private readonly StringBuilder _logStringBuilder = new StringBuilder();
        public string CurrentTitle { get; set; }
        public UpdateDataViewModel([NotNull]IErrorHandler errorHandler, [NotNull]ILogger<UpdateDataViewModel> logger, [NotNull]IAppClosingCancellationToken appClosingCancellationToken)
        {
            _logger = logger;
            _appClosingCancellationToken = appClosingCancellationToken;
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, errorHandler.HandleError);
            _currentThread = new Thread(DequeueLogs);
            _currentThread.Start();
        }

        private void DequeueLogs()
        {
            const int MAX_STRING_LENGTH = 10000;
            Thread.CurrentThread.Name = "Update logs thread";
            while (true)
            {
                if (_appClosingCancellationToken.CancellationTokenSource.IsCancellationRequested)
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
            await Task.Run(() => PendingLogs.Enqueue(message));
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
