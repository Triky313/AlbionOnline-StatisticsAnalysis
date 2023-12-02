namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainViewModel MainViewModel { get; }

    public MainWindowViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;
    }
}