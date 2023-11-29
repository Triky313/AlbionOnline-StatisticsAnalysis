using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

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
        Dispatcher.UIThread.Invoke(() => App.ServiceProvider?.GetRequiredService<ErrorBarViewModel>().Set(true, "WOW"), DispatcherPriority.Normal);
    }

    [ObservableProperty]
    private string _name;
}
