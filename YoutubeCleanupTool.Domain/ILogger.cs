namespace YouTubeCleanupTool.Domain
{
    public interface ILogger
    {
        public delegate void OnLog(string message);
        public event OnLog LogChanged;
        void Fatal(string message) => Log(LogLevel.Fatal, message);
        void Error(string message) => Log(LogLevel.Error, message);
        void Warning(string message) => Log(LogLevel.Warning, message);
        void Debug(string message) => Log(LogLevel.Debug, message);
        void Info(string message) => Log(LogLevel.Info, message);
        void Trace(string message) => Log(LogLevel.Trace, message);
        void Log(LogLevel category, string message);
        enum LogLevel
        {
            Trace,
            Info,
            Debug,
            Warning,
            Error,
            Fatal
        }
    }
}
