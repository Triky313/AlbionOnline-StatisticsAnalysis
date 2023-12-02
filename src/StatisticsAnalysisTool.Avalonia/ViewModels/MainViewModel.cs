using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ErrorBarViewModel ErrorBarViewModel { get; }
    public FooterViewModel FooterViewModel { get; }

    public MainViewModel(ErrorBarViewModel errorBarViewModel, FooterViewModel footerViewModel)
    {
        ErrorBarViewModel = errorBarViewModel;
        FooterViewModel = footerViewModel;
        _name = "Aaron";
    }

    [RelayCommand]
    public void Use()
    {
        //ErrorBarViewModel.Set(true, "WOW");
    }

    [ObservableProperty]
    private string _name;
}