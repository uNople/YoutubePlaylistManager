using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YouTubeCleanupWpf
{
    public class RunMethodCommand : ICommand
    {
        private readonly Func<WpfPlaylistData, Task> _action;

        public RunMethodCommand([NotNull]Func<WpfPlaylistData, Task> action)
        {
            _action = action;
        }
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public async void Execute(object? parameter)
        {
            if (parameter != null && parameter is WpfPlaylistData data)
            {
                await _action(data);
            }
        }

        public event EventHandler? CanExecuteChanged;
    }
}
