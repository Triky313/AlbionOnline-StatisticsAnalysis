using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Common;

public class AsyncCommandHandler : ICommand
{
    private readonly Func<object, Task> _execute;
    private readonly bool _canExecute;

    public AsyncCommandHandler(Func<object, Task> execute, bool canExecute)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
        return _canExecute;
    }

    public void Execute(object parameter)
    {
        _execute(parameter);
    }
}