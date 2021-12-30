using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

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
                WindowState = WindowState.Maximized;
                MaximizedButton.Content = 2;
                _isWindowMaximized = true;
                return;
            }

            if (e.ClickCount == 2 && WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                MainWindowViewModel.CenterWindowOnScreen();
                MaximizedButton.Content = 1;
                _isWindowMaximized = false;
            }
        }

        private void MaximizedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isWindowMaximized)
            {
                WindowState = WindowState.Normal;
                MainWindowViewModel.CenterWindowOnScreen();
                MaximizedButton.Content = 1;
                _isWindowMaximized = false;
            }
            else
            {
                WindowState = WindowState.Maximized;
                MaximizedButton.Content = 2;
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
            _mainWindowViewModel.SaveSettings(WindowState, RestoreBounds, Height, Width);

            if (_mainWindowViewModel.IsTrackingActive)
            {
                _mainWindowViewModel.StopTracking();
            }
        }
    }
}
