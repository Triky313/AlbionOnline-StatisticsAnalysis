using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        _name = "Aaron";
    }

    [RelayCommand]
    public void Use()
    {
        //App.ServiceProvider?.GetRequiredService<ErrorBarViewModel>().Set(true, "WOW");
    }

    [ObservableProperty]
    private string _name;
}
