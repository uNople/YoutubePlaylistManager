using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YouTubeCleanupWpf
{
    public class RunMethodCommand<T> : ICommand
    {
        private readonly Func<T, Task> _action;
        private readonly Action<Exception> _errorCallback;

        public RunMethodCommand([NotNull]Func<T, Task> action, [NotNull] Action<Exception> errorCallback)
        {
            _action = action;
            _errorCallback = errorCallback;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {
            if (parameter is T data)
            {
                try
                {
                    await _action(data);
                }
                catch (Exception ex)
                {
                    _errorCallback(ex);
                }
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
