using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Dungeon;
using System;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Common;

public class RemoveDungeonButtonClick : ICommand
{
    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        App.ServiceProvider.GetRequiredService<IDungeonController>()?.RemoveDungeon((string) parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add { }
        remove { }
    }
}