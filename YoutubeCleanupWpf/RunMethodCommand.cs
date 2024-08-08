#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YouTubeCleanupWpf;

public class RunMethodCommand<T>(Func<T, Task> action, Action<Exception> errorCallback) : ICommand
{
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
                await action(data);
            }
            catch (Exception ex)
            {
                errorCallback(ex);
            }
        }
    }

#pragma warning disable 067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 067
}