using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Views;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly MainWindowViewModel _mainWindowViewModel;
    private static bool _isWindowMaximized;

    public MainWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        _mainWindowViewModel = mainWindowViewModel;
        DataContext = _mainWindowViewModel;

        _ = SettingsController.SetMainWindowSettings();
    }

    private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left || e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (Exception exception)
        {
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Application.Current?.Shutdown();
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private async void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        await Task.Delay(200);
        switch (e.ClickCount)
        {
            case 2 when WindowState == WindowState.Normal:
                SwitchState();
                _isWindowMaximized = true;
                return;
            case 2 when WindowState == WindowState.Maximized:
                SwitchState();
                Utilities.CenterWindowOnScreen(this);
                _isWindowMaximized = false;
                break;
        }
    }

    private void MaximizedButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isWindowMaximized)
        {
            SwitchState();
            Utilities.CenterWindowOnScreen(this);
            _isWindowMaximized = false;
        }
        else
        {
            SwitchState();
            _isWindowMaximized = true;
        }
    }

    private void CopyPartyToClipboard_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        _mainWindowViewModel.TrackingController.EntityController.CopyPartyToClipboard();
    }

    private void MainWindow_OnClosed(object sender, EventArgs eventArgs)
    {
        _mainWindowViewModel.SaveLootLogger();
        SettingsController.SaveSettings(WindowState, Height, Width);

        if (_mainWindowViewModel.IsTrackingActive)
        {
            _ = _mainWindowViewModel.StopTrackingAsync();
        }
    }

    private void Grid_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (WindowState == WindowState.Maximized)
            {
                SwitchState();
                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Top = 3;
                    MaximizedButton.Content = 1;
                }
            }
            DragMove();
        }
    }

    private void SwitchState()
    {
        WindowState = WindowState switch
        {
            WindowState.Normal => WindowState.Maximized,
            WindowState.Maximized => WindowState.Normal,
            _ => WindowState
        };
    }

    private void BtnTryToLoadItemJsonAgain_Click(object sender, RoutedEventArgs e)
    {
        _mainWindowViewModel?.InitItemsAsync().ConfigureAwait(false);
    }

    private void ToolTasksCloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        _mainWindowViewModel?.SetToolTasksVisibility(Visibility.Collapsed);
    }

    private void ToolTasksOpenClose_PreviewMouseDown(object sender, RoutedEventArgs e)
    {
        _mainWindowViewModel?.SwitchToolTasksState();
    }

    private void OpenToolDirectory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = Process.Start(new ProcessStartInfo { FileName = MainWindowViewModel.ToolDirectory, UseShellExecute = true });
        }
        catch (Exception exception)
        {
            _ = MessageBox.Show(exception.Message, LanguageController.Translation("ERROR"));
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, exception);
        }
    }
}