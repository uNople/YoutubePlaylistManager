#nullable enable
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace YouTubeCleanup.Ui;

public class RunMethodWithoutParameterCommand(Func<Task> action, Action<Exception> errorCallback)
    : ICommand
{
    public bool CanExecute(object? parameter)
    {
        return true;
    }

    public async void Execute(object? parameter)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            errorCallback(ex);
        }
    }

#pragma warning disable 0067
    public event EventHandler? CanExecuteChanged;
#pragma warning restore 0067
}