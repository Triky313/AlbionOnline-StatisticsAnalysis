using StatisticsAnalysisTool.Network.Manager;
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
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.DungeonController?.RemoveDungeon((string) parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add { }
        remove { }
    }
}