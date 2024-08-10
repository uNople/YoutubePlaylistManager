using UsedImplicitly = JetBrains.Annotations.UsedImplicitlyAttribute;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using YouTubeCleanupTool.Domain;

namespace YouTubeCleanup.Ui;

public class UpdateDataViewModel : INotifyPropertyChanged, IUpdateDataViewModel
{
    private readonly ILogger _logger;
    private readonly IAppClosingCancellationToken _appClosingCancellationToken;
    private readonly IDoWorkOnUi _doWorkOnUi;
#pragma warning disable 067
    public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 067
    [UsedImplicitly] public string LogText { get; set; }
    [UsedImplicitly] public ICommand CloseCommand { get; set; }
    [UsedImplicitly] public ICommand CancelActiveTasksCommand { get; set; }
    public IUpdateDataWindow ParentWindow { get; set; }
    private ConcurrentQueue<string> UiLogs { get; } = new();
    private Thread _writeLogsToUiThread;
    private readonly StringBuilder _logStringBuilder = new();
    [UsedImplicitly] public string CurrentTitle { get; set; }

    public Dictionary<Guid, CancellableJob> ActiveJobs { get; } = new();
    private SemaphoreSlim ActiveJobsSemaphore { get; } = new(1, 1);
    [UsedImplicitly] public bool TasksRunning { get; set; }
    private int _waiting;
    private int _gained;
    private int _released;
    private int _timedOut;
    private int _inProgress;
    [UsedImplicitly] public bool IsProgressBarIndeterminate { get; set; }
    [UsedImplicitly] public int ProgressBarValue { get; set; }
    [UsedImplicitly] public int ProgressBarMaxValue { get; set; }

    public UpdateDataViewModel([NotNull] IErrorHandler errorHandler,
        [NotNull] IAppClosingCancellationToken appClosingCancellationToken,
        [NotNull] IDoWorkOnUi doWorkOnUi,
        [NotNull] ILogger logger)
    {
        _logger = logger;
        _appClosingCancellationToken = appClosingCancellationToken;
        _doWorkOnUi = doWorkOnUi;
        CloseCommand = new RunMethodWithoutParameterCommand(Hide, errorHandler.HandleError);
        CancelActiveTasksCommand = new RunMethodWithoutParameterCommand(CancelActiveTasks, errorHandler.HandleError);
        _writeLogsToUiThread = new Thread(WriteLogsToUi);
        _writeLogsToUiThread.Start();
        logger.LogChanged += (message) => UiLogs.Enqueue(message);
    }

    private void WriteLogsToUi()
    {
        const int maxStringLength = 10000;
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
                _logStringBuilder.Insert(0, message + Environment.NewLine);
                shouldAppend = true;
            }

            if (shouldAppend)
            {
                // Clamp the string builder to 10,000 characters (better than creating yet another string to do the truncate on)
                // We still need to clamp the text's length to get a responsive UI in WPF
                if (_logStringBuilder.Length > maxStringLength)
                    _logStringBuilder.Remove(maxStringLength, _logStringBuilder.Length - maxStringLength);

                var logText = _logStringBuilder.ToString();
                _doWorkOnUi.RunOnUiThreadSync(() => LogText = logText);
            }

            Thread.Sleep(100);
        }
    }

    public async Task CreateNewActiveTask(Guid runGuid, string title, CancellationTokenSource cancellationTokenSource)
    {
        await DoActiveTaskWork(
            async () => await Task.Run(() => ActiveJobs[runGuid] = new CancellableJob
                { Id = runGuid, Name = title, CancellationTokenSource = cancellationTokenSource }),
            extraMessage: $"guid {runGuid} title {title}");
    }

    public async Task SetActiveTaskComplete(Guid runGuid, string title)
    {
        await DoActiveTaskWork(async () => await Task.Run(() => ActiveJobs.Remove(runGuid)),
            extraMessage: $"guid {runGuid} title {title}");
    }

    public async Task IncrementProgress()
    {
        await _doWorkOnUi.RunOnUiThreadAsync(() => ProgressBarValue++);
    }

    public async Task SetNewProgressMax(int progressBarMaxValue)
    {
        await _doWorkOnUi.RunOnUiThreadAsync(() =>
        {
            IsProgressBarIndeterminate = false;
            ProgressBarValue = 0;
            ProgressBarMaxValue = progressBarMaxValue;
        });
    }

    public async Task ResetProgress()
    {
        await _doWorkOnUi.RunOnUiThreadAsync(() => { ProgressBarValue = 0; });
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
    private async Task DoActiveTaskWork(Func<Task> act, [CallerMemberName] string callingMethod = default,
        string extraMessage = default)
    {
        const int perThreadTimeoutTimeMs = 500;
        Interlocked.Increment(ref _inProgress);
        Interlocked.Increment(ref _waiting);

        void LogStats(string message) =>
            _logger.Debug(
                $"{message} - In Progress: {_inProgress}, waiting: {_waiting}, gained: {_gained}, released: {_released}, Timed out: {_timedOut} - Task work: {callingMethod} - {extraMessage}");

        if (await ActiveJobsSemaphore.WaitAsync(perThreadTimeoutTimeMs))
        {
            Interlocked.Increment(ref _gained);
            try
            {
                await act();
                TasksRunning = ActiveJobs.Count != 0;
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
            _logger.Error(
                $"{callingMethod}: Couldn't gain access to lock within {perThreadTimeoutTimeMs}ms.{(!string.IsNullOrEmpty(extraMessage) ? $" {extraMessage}" : "")}");
            LogStats("Extra information:");
        }

        Interlocked.Decrement(ref _inProgress);
        LogStats("Finish");
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