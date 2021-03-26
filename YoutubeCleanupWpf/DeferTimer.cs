using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace YouTubeCleanupWpf
{
    public class DeferTimer
    {
        private Timer _timer;
        private readonly Func<Task> _action;
        private readonly Action<Exception> _errorHandler;

        public DeferTimer([NotNull]Func<Task> action, [NotNull] Action<Exception> errorHandler)
        {
            _action = action;
            _errorHandler = errorHandler;
        }

        public void DeferByMilliseconds(int deferTime)
        {
            _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            _timer = new Timer(RunAction, null, deferTime, Timeout.Infinite);
        }

        private void RunAction(object state)
        {
            // Hmmmm. Not sure about this. Mixing timer and async might be bad?
            // TODO: Will this timer even fire if the app is under heavy load?
            Task.Run(async () => await RunAction());
        }

        private async Task RunAction()
        {
            try
            {
                await _action();
            }
            catch (Exception ex)
            {
                _errorHandler(ex);
            }
        }
    }
}
