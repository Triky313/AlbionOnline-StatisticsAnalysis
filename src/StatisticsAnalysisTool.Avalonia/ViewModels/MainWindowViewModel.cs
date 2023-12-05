using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using StatisticsAnalysisTool.Avalonia.ToolSettings;
using System;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public FooterViewModel FooterViewModel { get; }
    private readonly ISettingsController _settingsController;

    public MainWindowViewModel(IErrorBarViewModel errorBarViewModel, FooterViewModel footerViewModel, ISettingsController settingsController)
    {
        _errorBarViewModel = errorBarViewModel;
        FooterViewModel = footerViewModel;
        _settingsController = settingsController;

        InitWindow();
    }

    private void InitWindow()
    {
#if DEBUG
        IsDebugMode = true;
#endif

        // Window
        WindowState = _settingsController.CurrentUserSettings.MainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
        Height = _settingsController.CurrentUserSettings.MainWindowHeight;
        Width = _settingsController.CurrentUserSettings.MainWindowWidth;

        // Unsupported OS
        IsUnsupportedOs = Environment.OSVersion.Version.Major >= 10;
    }

    public void SetWindowSettings(PixelPoint position)
    {
        _settingsController.SetWindowSettings(WindowState, Height, Width, position);
    }

    [ObservableProperty]
    private IErrorBarViewModel _errorBarViewModel;

    [ObservableProperty]
    private bool _isDebugMode;

    [ObservableProperty]
    private WindowState _windowState;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double _width;
    
    [ObservableProperty]
    private bool _isUnsupportedOs;

    [ObservableProperty]
    private string _serverTypeText = "SERVER_TYPE";
}