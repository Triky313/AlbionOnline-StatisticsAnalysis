using CommunityToolkit.Mvvm.ComponentModel;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class ErrorBarViewModel : ViewModelBase
{
    public ErrorBarViewModel()
    {
        _text = string.Empty;
        _isVisible = false;
    }

    public void Set(bool isVisible, string errorMessage)
    {
        _text = errorMessage;
        _isVisible = isVisible;
    }

    public void Close(string parameter)
    {
        _text = string.Empty;
        _isVisible = false;
    }

    public bool CanRunClose(object parameter)
    {
        return true;
    }

    [ObservableProperty]
    private string _text;

    [ObservableProperty]
    private bool _isVisible;
}