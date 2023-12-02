using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class ErrorBarViewModel : ViewModelBase
{
    public void Set(bool isVisible, string errorMessage)
    {
        Text = errorMessage;
        IsVisible = isVisible;
    }

    [RelayCommand]
    public void Close()
    {
        IsVisible = false;
        Text = string.Empty;
    }

    [ObservableProperty]
    private string _text = string.Empty;

    [ObservableProperty]
    private bool _isVisible;
}