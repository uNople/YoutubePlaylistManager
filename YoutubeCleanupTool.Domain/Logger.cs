using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;

namespace YouTubeCleanupTool.Domain;

public class Logger : ILogger
{
    private readonly IAppClosingCancellationToken _appClosingCancellationToken;
    private Thread _writeLogsToDiskThread;
    private ConcurrentQueue<string> DiskLogs { get; } = new();
    private string LogFile { get; set; } = "Log.txt";
    private readonly int _currentProcessId;

    public Logger(
        [NotNull] IAppClosingCancellationToken appClosingCancellationToken)
    {
        _appClosingCancellationToken = appClosingCancellationToken;
        _currentProcessId = Process.GetCurrentProcess().Id;
        _writeLogsToDiskThread = new Thread(WriteLogsToDisk);
        _writeLogsToDiskThread.Start();
    }

    private void WriteLogsToDisk()
    {
        Thread.CurrentThread.Name = "Update logs to disk thread";
        // This will just fall back to the filename, so it'll be in whatever directory the exe is in
        string path = CalculateLogFileName();
        while (true)
        {
            if (_appClosingCancellationToken.CancellationTokenSource.IsCancellationRequested)
            {
                Log(ILogger.LogLevel.Trace, "App exiting");
                // Try to write logs on app exit
                // If it fails, it's no biggie, since we're exiting anyway
                TryWriteLogs(path);
                _writeLogsToDiskThread.Interrupt();
                _writeLogsToDiskThread = null;
                return;
            }

            if (!TryWriteLogs(path))
            {
                path = CalculateLogFileName();
            }

            Thread.Sleep(100);
        }
    }

    private bool TryWriteLogs(string path)
    {
        var success = true;
        var messages = new List<string>();
        while (DiskLogs.TryDequeue(out var message))
        {
            messages.Add(message);
        }

        if (messages.Count != 0)
        {
            RollLogs(path);

            try
            {
                File.AppendAllLines(path, messages);
            }
            catch (Exception ex)
            {
                Log(ILogger.LogLevel.Error, $"Access to path {path} denied. Error: {ex}");
                messages.ForEach(DiskLogs.Enqueue);
                LogFile = $"Log.txt.{PathSafeLocalCurrentDate()}";
                success = false;
            }
        }

        return success;
    }

    private void RollLogs(string path)
    {
        if (!File.Exists(path))
            return;

        const double bitAmountInBytes = 1024d;
        const int rollLogsMegabytesAmount = 2;

        try
        {
            var fileLengthMb = new FileInfo(path).Length / bitAmountInBytes / bitAmountInBytes;
            if (fileLengthMb >= rollLogsMegabytesAmount)
            {
                File.Move(path, $"{path}.{PathSafeLocalCurrentDate()}");
            }
        }
        catch (Exception ex)
        {
            Log(ILogger.LogLevel.Error,
                $"Error when rolling logs at cutoff {rollLogsMegabytesAmount}MB for path '{path}' on disk. Error: {ex}");
        }
    }

    private string PathSafeLocalCurrentDate()
    {
        return DateTime.Now.ToString("o").Replace(":", ".");
    }

    private string CalculateLogFileName() =>
        Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            LogFile);

    public event ILogger.OnLog LogChanged;

    public void Log(ILogger.LogLevel category, string message)
    {
        var formattedMessage =
            $"{DateTime.Now:o} [{_currentProcessId}] [{Thread.CurrentThread.ManagedThreadId}] [{Thread.CurrentThread.Name}] {message}";
        DiskLogs.Enqueue(formattedMessage);
        LogChanged?.Invoke(message);
    }
}