using System.Windows.Input;

namespace Finstat.Desktop.Core.ViewModels;

public class AsyncCommand(Func<Task> execute) : ICommand
{
    private bool isRunning;

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter)
    {
        return !isRunning;
    }

    public async void Execute(object? parameter)
    {
        await ExecuteAsync();
    }

    public async Task ExecuteAsync()
    {
        if (isRunning)
            return;
        isRunning = true;
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        try
        {
            await execute();
        }
        finally
        {
            isRunning = false;
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

public class AsyncCommand<T>(Func<T, Task> execute) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter)
    {
        return parameter is T;
    }

    public async void Execute(object? parameter)
    {
        if (parameter is T value)
            await ExecuteAsync(value);
    }

    public async Task ExecuteAsync(T parameter)
    {
        await execute(parameter);
    }
}
