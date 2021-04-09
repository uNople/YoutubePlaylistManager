using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using YouTubeCleanupTool.Domain;
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
        public ICommand CancelActiveTasksCommand { get; set; }
        public UpdateDataWindow ParentWindow { get; set; }
        public MainWindowViewModel MainWindowViewModel { get; set; }
        private ConcurrentQueue<string> UiLogs { get; } = new();
        private ConcurrentQueue<string> DiskLogs { get; } = new();
        private Thread _writeLogsToUiThread;
        private Thread _writeLogsToDiskThread;
        private readonly StringBuilder _logStringBuilder = new StringBuilder();
        public string CurrentTitle { get; set; }
        private Dictionary<Guid, CancellableJob> ActiveJobs { get; set; } = new();
        private SemaphoreSlim ActiveJobsSemaphore { get; set; } = new(1, 1);
        public bool TasksRunning { get; set; }
        private int _waiting;
        private int _gained;
        private int _released;
        private int _timedOut;
        private int _inProgress;

        public UpdateDataViewModel([NotNull]IErrorHandler errorHandler, [NotNull]ILogger<UpdateDataViewModel> logger, [NotNull]IAppClosingCancellationToken appClosingCancellationToken)
        {
            _logger = logger;
            _appClosingCancellationToken = appClosingCancellationToken;
            CloseCommand = new RunMethodWithoutParameterCommand(Hide, errorHandler.HandleError);
            CancelActiveTasksCommand = new RunMethodWithoutParameterCommand(CancelActiveTasks, errorHandler.HandleError);
            _writeLogsToUiThread = new Thread(WriteLogsToUi);
            _writeLogsToUiThread.Start();
            _writeLogsToDiskThread = new Thread(WriteLogsToDisk);
            _writeLogsToDiskThread.Start();
        }

        private void WriteLogsToDisk()
        {
            Thread.CurrentThread.Name = "Update logs to disk thread";
            const string logFile = "Log.txt";
            // This will just fall back to the filename, so it'll be in whatever directory the exe is in
            var path = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty, logFile);
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

                try
                {
                    File.AppendAllLines(path, messages);
                }
                catch (Exception ex)
                {
                    UiLogs.Enqueue($"Access to path {path} denied. Error: {ex}");
                    messages.ForEach(DiskLogs.Enqueue);
                }

                Thread.Sleep(100);
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

        public async Task CreateNewActiveTask(Guid runGuid, string title, CancellationTokenSource cancellationTokenSource)
        {
            await DoActiveTaskWork(async () => await Task.Run(() => ActiveJobs[runGuid] = new CancellableJob {Id = runGuid, Name = title, CancellationTokenSource = cancellationTokenSource}),
                extraMessage: $"guid {runGuid} title {title}");
        }

        public async Task SetActiveTaskComplete(Guid runGuid, string title, CancellationTokenSource cancellationTokenSource)
        {
            await DoActiveTaskWork(async () => await Task.Run(() => ActiveJobs.Remove(runGuid)), extraMessage: $"guid {runGuid} title {title}");
        }

        public async Task CancelActiveTasks()
        {
            await DoActiveTaskWork(async () => await Task.Run(() => 
            {
                foreach (var item in ActiveJobs)
                {
                    item.Value.Cancel();
                }
                ActiveJobs.Clear();
                TasksRunning = false;
            }), extraMessage: "Cancelling active tasks");
        }

        /// <summary>
        /// Run a task which interacts with the ActiveJobs dictionary.
        /// This was intended for a few things:
        /// 1. Be thread safe when interacting with the dictionary
        /// 2. Have a timeout which is obeyed, so things don't end up locking and causing problems
        ///    NOTE: it's not really an issue if we time out and don't either queue up the Job, or
        ///          time out and fail to cancel every job. We can always retry
        /// 3. Play with semaphores again
        /// 4. Have some useful debug information written if things go wrong. We have counts of where
        ///    we got to in the process, so we can at least see where we got stuck  
        /// </summary>
        /// <param name="act"></param>
        /// <param name="callingMethod"></param>
        /// <param name="extraMessage"></param>
        private async Task DoActiveTaskWork(Func<Task> act, [CallerMemberName]string callingMethod = default, string extraMessage = default)
        {
            const int perThreadTimeoutTimeMs = 500;
            Interlocked.Increment(ref _inProgress);
            Interlocked.Increment(ref _waiting);
            
            async Task Log(string message) => await PrependText($"{message} - In Progress: {_inProgress}, waiting: {_waiting}, gained: {_gained}, released: {_released}, Timed out: {_timedOut} - Task work: {callingMethod} - {extraMessage}");

            if (await ActiveJobsSemaphore.WaitAsync(perThreadTimeoutTimeMs))
            {
                Interlocked.Increment(ref _gained);
                try
                {
                    await act();
                    TasksRunning = ActiveJobs.Any();
                }
                finally
                {
                    Interlocked.Increment(ref _released);
                    ActiveJobsSemaphore.Release();
                }
            }
            else
            {
                Interlocked.Increment(ref _timedOut);
                await PrependText($"{callingMethod}: Couldn't gain access to lock within {perThreadTimeoutTimeMs}ms.{(!string.IsNullOrEmpty(extraMessage) ? $" {extraMessage}" : "")}");
                await Log("Extra information:");
            }

            Interlocked.Decrement(ref _inProgress);
            await Log("Finish");
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
}
