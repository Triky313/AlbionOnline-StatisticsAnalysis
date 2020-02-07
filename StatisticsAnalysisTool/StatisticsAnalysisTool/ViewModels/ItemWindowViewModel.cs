namespace StatisticsAnalysisTool.ViewModels
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using Views;

    public class ItemWindowViewModel: INotifyPropertyChanged
    {
        private ItemWindow _mainWindow;

        public ItemWindowViewModel(ItemWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }
}