using CommunityToolkit.Mvvm.ComponentModel;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        _name = "Aaron";
    }

    public string Greeting => "Welcome to Avalonia!";

    [ObservableProperty]
    private string _name;
}
