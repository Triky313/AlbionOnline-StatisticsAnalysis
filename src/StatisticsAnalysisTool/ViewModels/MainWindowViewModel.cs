using StatisticsAnalysisTool.Common.UserSettings;
using System.Windows;

namespace StatisticsAnalysisTool.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly ISettingsController _settingsController;
    private WindowState _windowState = WindowState.Normal;
    private double _top;
    private double _left;
    private double _height;
    private double _width;

    public MainWindowViewModel(ISettingsController settingsController)
    {
        _settingsController = settingsController;

        InitWindow();
    }

    private void InitWindow()
    {
        WindowState = SettingsController.CurrentSettings.MainWindowMaximized ? WindowState.Maximized : WindowState.Normal;
        Height = SettingsController.CurrentSettings.MainWindowHeight;
        Width = SettingsController.CurrentSettings.MainWindowWidth;
        Left = SettingsController.CurrentSettings.MainWindowLeftPosition;
        Top = SettingsController.CurrentSettings.MainWindowTopPosition;
    }

    public void ClosingWindow()
    {
        _settingsController.SetWindowSettings(WindowState, Height, Width, Left, Top);
    }

    public WindowState WindowState
    {
        get => _windowState;
        set
        {
            _windowState = value;
            OnPropertyChanged();
        }
    }

    public double Top
    {
        get => _top;
        set
        {
            _top = value;
            OnPropertyChanged();
        }
    }

    public double Left
    {
        get => _left;
        set
        {
            _left = value;
            OnPropertyChanged();
        }
    }

    public double Height
    {
        get => _height;
        set
        {
            _height = value;
            OnPropertyChanged();
        }
    }

    public double Width
    {
        get => _width;
        set
        {
            _width = value;
            OnPropertyChanged();
        }
    }
}