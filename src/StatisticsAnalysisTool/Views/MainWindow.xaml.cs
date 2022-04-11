using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static bool _isWindowMaximized;

        public MainWindow()
        {
            InitializeComponent();
            _mainWindowViewModel = new MainWindowViewModel(this);
            DataContext = _mainWindowViewModel;
        }

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                DragMove();
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

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && WindowState == WindowState.Normal)
            {
                SwitchState();
                _isWindowMaximized = true;
                return;
            }

            if (e.ClickCount == 2 && WindowState == WindowState.Maximized)
            {
                SwitchState();
                Utilities.CenterWindowOnScreen(this);
                _isWindowMaximized = false;
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

        private void ResetParty_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _ = _mainWindowViewModel.ResetPartyAsync();
        }

        private void MainWindow_OnClosed(object sender, EventArgs eventArgs)
        {
            _mainWindowViewModel.SaveLootLogger();
            SettingsController.SaveSettings(WindowState, Height, Width);

            if (_mainWindowViewModel.IsTrackingActive)
            {
                _mainWindowViewModel.StopTracking();
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

        private void BtnTryToLoadTheItemListAgain_Click(object sender, RoutedEventArgs e)
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
    }
}
