using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Common
{
    public class RemoveDungeonButtonClick : ICommand
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public RemoveDungeonButtonClick(MainWindowViewModel mainWindowViewModel)
        {
            _mainWindowViewModel = mainWindowViewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _mainWindowViewModel.TrackingController.DungeonController.RemoveDungeon((string)parameter);
        }

        public event EventHandler CanExecuteChanged 
        {
            add {}
            remove {}
        }
    }
}