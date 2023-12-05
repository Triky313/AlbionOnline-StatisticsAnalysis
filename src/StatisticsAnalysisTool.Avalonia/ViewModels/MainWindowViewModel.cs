using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using StatisticsAnalysisTool.Avalonia.ToolSettings;
using StatisticsAnalysisTool.Avalonia.ViewModels.Interfaces;
using System;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public FooterViewModel FooterViewModel { get; }
    private readonly ISettingsController _settingsController;

    public MainWindowViewModel(IMainWindowHeaderViewModel mainWindowHeaderViewModel, 
        IErrorBarViewModel errorBarViewModel, 
        FooterViewModel footerViewModel, 
        ISettingsController settingsController)
    {
        _mainWindowHeaderViewModel = mainWindowHeaderViewModel;
        _errorBarViewModel = errorBarViewModel;
        FooterViewModel = footerViewModel;
        _settingsController = settingsController;

        InitWindow();
    }

    private void InitWindow()
    {
        // Window
        WindowState = _settingsController.CurrentUserSettings.MainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
        Height = _settingsController.CurrentUserSettings.MainWindowHeight;
        Width = _settingsController.CurrentUserSettings.MainWindowWidth;
    }

    public void SetWindowSettings(PixelPoint position)
    {
        _settingsController.SetWindowSettings(WindowState, Height, Width, position);
    }

    [ObservableProperty]
    private IMainWindowHeaderViewModel _mainWindowHeaderViewModel;

    [ObservableProperty]
    private IErrorBarViewModel _errorBarViewModel;

    [ObservableProperty]
    private WindowState _windowState;

    [ObservableProperty]
    private double _height;

    [ObservableProperty]
    private double _width;
}