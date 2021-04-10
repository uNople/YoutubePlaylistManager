#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YouTubeCleanupWpf.Converters
{
    public class RunMethodWithoutParameterCommand : ICommand
    {
        private readonly Func<Task> _action;
        private readonly Action<Exception> _errorCallback;

        public RunMethodWithoutParameterCommand([NotNull] Func<Task> action, [NotNull] Action<Exception> errorCallback)
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
            try
            {
                await _action();
            }
            catch (Exception ex)
            {
                _errorCallback(ex);
            }
        }
        
        #pragma warning disable 0067
        public event EventHandler? CanExecuteChanged;
        #pragma warning restore 0067
    }
}
