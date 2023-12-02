using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using StatisticsAnalysisTool.Avalonia.ToolSettings;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ISettingsController _settingsController;
    public MainViewModel MainViewModel { get; }

    public MainWindowViewModel(MainViewModel mainViewModel, ISettingsController settingsController)
    {
        _settingsController = settingsController;
        MainViewModel = mainViewModel;
        InitWindow();
    }

    private void InitWindow()
    {
        WindowState = _settingsController.CurrentUserSettings.MainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
        Height = _settingsController.CurrentUserSettings.MainWindowHeight;
        Width = _settingsController.CurrentUserSettings.MainWindowWidth;
    }

    public void SetWindowSettings(PixelPoint position)
    {
        _settingsController.SetWindowSettings(WindowState, Height, Width, position);
    }
    
    [ObservableProperty]
    private WindowState _windowState;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double _width;
}