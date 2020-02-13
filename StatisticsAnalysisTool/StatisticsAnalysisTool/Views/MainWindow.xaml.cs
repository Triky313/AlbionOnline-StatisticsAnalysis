using FontAwesome.WPF;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace StatisticsAnalysisTool.Views
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel _mainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            _mainWindowViewModel = new MainWindowViewModel(this);
            DataContext = _mainWindowViewModel;
        }

        private void TxtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            _mainWindowViewModel.LoadLvItems(TxtSearch.Text);
        }

        private void LvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (Item) ((ListView) sender).SelectedValue;
            var iw = new ItemWindow(item);
            iw.Show();
        }

        private void ImageAwesome_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var sw = new SettingsWindow();
            sw.ShowDialog();
        }

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void ImageAwesome_MouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is ImageAwesome icon)
                icon.Spin = true;
        }

        private void ImageAwesome_MouseLeave(object sender, MouseEventArgs e)
        {
            if (sender is ImageAwesome icon)
                icon.Spin = false;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                return;
            }

            if (e.ClickCount == 2 && WindowState == WindowState.Maximized) WindowState = WindowState.Normal;
        }

        private static Rect _storedSize;
        private static bool _isWindowMaximized;

        private void MaximizedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isWindowMaximized)
            {
                Left = _storedSize.Left;
                Top = _storedSize.Top;
                Height = _storedSize.Height;
                Width = _storedSize.Width;
                _mainWindowViewModel.CenterWindowOnScreen();
                MaximizedButton.Content = 1;
                _isWindowMaximized = false;
            }
            else
            {
                _storedSize.Width = Width;
                _storedSize.Height = Height;
                Left = SystemParameters.WorkArea.Left;
                Top = SystemParameters.WorkArea.Top;
                Height = SystemParameters.WorkArea.Height;
                Width = SystemParameters.WorkArea.Width;
                MaximizedButton.Content = 2;
                _isWindowMaximized = true;
            }
        }

        private void CbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mode = _mainWindowViewModel.ModeSelection;

            switch (mode.ViewMode)
            {
                case MainWindowViewModel.ViewMode.Normal:
                    HideAllGrids();
                    GridNormalMode.Visibility = Visibility.Visible;
                    TxtSearch.Focus();
                    return;
                case MainWindowViewModel.ViewMode.Player:
                    HideAllGrids();
                    GridPlayerMode.Visibility = Visibility.Visible;
                    TxtBoxPlayerModeUsername.Focus();
                    return;
                case MainWindowViewModel.ViewMode.Gold:
                    HideAllGrids();
                    GridGoldMode.Visibility = Visibility.Visible;
                    return;
            }
        }

        private void HideAllGrids()
        {
            GridNormalMode.Visibility = Visibility.Hidden;
            GridPlayerMode.Visibility = Visibility.Hidden;
            GridGoldMode.Visibility = Visibility.Hidden;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                Settings.Default.MainWindowHeight = RestoreBounds.Height;
                Settings.Default.MainWindowWidth = RestoreBounds.Width;
                Settings.Default.MainWindowMaximized = true;
            }
            else
            {
                Settings.Default.MainWindowHeight = Height;
                Settings.Default.MainWindowWidth = Width;
                Settings.Default.MainWindowMaximized = false;
            }

            Settings.Default.SavedPlayerInformationName = TxtBoxPlayerModeUsername.Text;
            Settings.Default.Save();
        }

        private async void BtnPlayerModeSave_Click(object sender, RoutedEventArgs e)
        {
            await _mainWindowViewModel.SetComparedPlayerModeInfoValues();
        }

        private async void TxtBoxPlayerModeUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            await _mainWindowViewModel.SetComparedPlayerModeInfoValues();
        }

        private void TxtBoxGoldModeAmountValues_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(_mainWindowViewModel.TextBoxGoldModeNumberOfValues, out var numberOfValues))
                _mainWindowViewModel.SetGoldChart(numberOfValues);
        }
    }
}