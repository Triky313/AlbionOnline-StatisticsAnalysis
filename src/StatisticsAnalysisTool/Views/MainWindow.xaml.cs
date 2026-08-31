using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly WindowChromeController _windowChromeController;
    private readonly DispatcherTimer _applicationUptimeTimer;

    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        _applicationUptimeTimer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _applicationUptimeTimer.Tick += ApplicationUptimeTimer_OnTick;
        _windowChromeController = new WindowChromeController(
            this,
            MaximizedButton,
            ResizeMode.CanResizeWithGrip,
            ResizeMode.NoResize);
        InitWindow();
        _mainWindowViewModel = mainWindowViewModel;
        DataContext = _mainWindowViewModel;
        UpdateApplicationUptime();
        _applicationUptimeTimer.Start();
    }

    public void InitWindow()
    {
        Height = SettingsController.CurrentSettings.MainWindowHeight;
        Width = SettingsController.CurrentSettings.MainWindowWidth;
        Left = SettingsController.CurrentSettings.MainWindowLeftPosition;
        Top = SettingsController.CurrentSettings.MainWindowTopPosition;
        if (SettingsController.CurrentSettings.MainWindowMaximized)
        {
            WindowState = WindowState.Maximized;
        }

        if (SettingsController.CurrentSettings.MainWindowLeftPosition == 0 && SettingsController.CurrentSettings.MainWindowLeftPosition == 0)
        {
            Utilities.CenterWindowOnScreen(this);
        }
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        _windowChromeController.DragMoveOnMouseDown(e);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current?.Shutdown();
    }

    private void MainWindow_OnClosing(object sender, EventArgs eventArgs)
    {
        _applicationUptimeTimer.Stop();
        _mainWindowViewModel.DisposeItemDetails();
        _mainWindowViewModel.CraftingBindings.DisposeLossExplorer();
        SettingsController.SetWindowSettings(WindowState, Height, Width, Left, Top);
    }

    private void ApplicationUptimeTimer_OnTick(object sender, EventArgs e)
    {
        UpdateApplicationUptime();
    }

    private void UpdateApplicationUptime()
    {
        ApplicationUptimeLabel.Content = App.ApplicationUptime.ToTimerString();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        _windowChromeController.ToggleMaximize();
    }

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _windowChromeController.ToggleMaximizeOnDoubleClick(e);
    }

    private void CopyPartyToClipboard_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.EntityController?.CopyPartyToClipboard();
    }

    private void TatsDropDownOpenClose_PreviewMouseDown(object sender, RoutedEventArgs e)
    {
        _mainWindowViewModel?.SwitchStatsDropDownState();
    }
}