using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YouTubeCleanup.Ui;

namespace YouTubeCleanupTool.Domain
{
    public class Logger : ILogger
    {
        private readonly IAppClosingCancellationToken _appClosingCancellationToken;
        private Thread _writeLogsToDiskThread;
        private ConcurrentQueue<string> DiskLogs { get; } = new();
        private string LOG_FILE { get; set; } = "Log.txt";
        public Logger(
            [NotNull] IAppClosingCancellationToken appClosingCancellationToken)
        {
            _appClosingCancellationToken = appClosingCancellationToken;
            _writeLogsToDiskThread = new Thread(WriteLogsToDisk);
            _writeLogsToDiskThread.Start();
        }

        private void WriteLogsToDisk()
        {
            Thread.CurrentThread.Name = "Update logs to disk thread";
            // This will just fall back to the filename, so it'll be in whatever directory the exe is in
            string CalculateLogFileName() => Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty, LOG_FILE);
            var path = CalculateLogFileName();
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
                    Log(ILogger.LogLevel.Error, $"Access to path {path} denied. Error: {ex}");
                    messages.ForEach(DiskLogs.Enqueue);
                    LOG_FILE = $"Log.txt.{DateTime.Now.ToString("o").Replace(":", ".")}";
                    path = CalculateLogFileName();
                }

                Thread.Sleep(100);
            }
        }

        public event ILogger.OnLog LogChanged;

        public void Log(ILogger.LogLevel category, string message)
        {
            var formattedMessage = $"{DateTime.Now:o} [{Thread.CurrentThread.ManagedThreadId}] {message}";
            DiskLogs.Enqueue(formattedMessage);
            LogChanged?.Invoke(message);
        }
    }
}
