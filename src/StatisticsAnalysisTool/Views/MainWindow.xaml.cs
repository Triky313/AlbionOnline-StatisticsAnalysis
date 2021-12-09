using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindowNew.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static bool _isWindowMaximized;

        public MainWindow()
        {
            InitializeComponent();
            _mainWindowViewModel = new MainWindowViewModel(this);
            DataContext = _mainWindowViewModel;
        }

        private void LvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (Item)((ListView)sender).SelectedValue;

            MainWindowViewModel.OpenItemWindow(item);
        }

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.SaveLootLogger();
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void StopLoadFullItemInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.IsFullItemInfoLoading = false;
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

        // ReSharper disable once UnusedMember.Local
        private void TxtBoxGoldModeAmountValues_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
        }

        private void TxtBoxMinSellPriceIsUndercutPrice_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
        }

        // ReSharper disable once UnusedMember.Local
        private void LoadGoldPrice_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(_mainWindowViewModel.TextBoxGoldModeNumberOfValues, out var numberOfValues))
            {
                _mainWindowViewModel.SetGoldChart(numberOfValues);
            }
        }

        private void FilterReset_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.ItemFilterReset();
        }

        private void LoadFullItemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.LoadAllFullItemInformationFromWeb();
        }

        private void AlertModeAlertActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.ToggleAlertSender(sender);
        }

        private void TrackingModeActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.TrackerActivationToggle();
        }

        private void BtnTrackingReset_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.ResetMainCounters();
        }

        private void BtnDamageMeterReset_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.ResetDamageMeter();
        }

        private void BtnDungeonTrackingReset_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.ResetDungeonCounters();
            _mainWindowViewModel.ResetDungeons();
        }

        private void BtnDeleteSelectedDungeons_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.DeleteSelectedDungeons();
        }

        private void OpenDamageMeterInfoPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            _mainWindowViewModel.IsDamageMeterPopupVisible = Visibility.Visible;
        }

        private void CloseDamageMeterInfoPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            _mainWindowViewModel.IsDamageMeterPopupVisible = Visibility.Hidden;
        }

        private void OpenMainTrackerInfoPopup_MouseEnter(object sender, MouseEventArgs e)
        {
            _mainWindowViewModel.IsMainTrackerPopupVisible = Visibility.Visible;
        }

        private void CloseMainTrackerInfoPopup_MouseLeave(object sender, MouseEventArgs e)
        {
            _mainWindowViewModel.IsMainTrackerPopupVisible = Visibility.Hidden;
        }

        private void MouseUp_FoldUnfoldDungeonStats(object sender, MouseEventArgs e)
        {
            _mainWindowViewModel.DungeonStatsGridToggle();
        }

        private void BtnTrackingNotificationsReset_Click(object sender, RoutedEventArgs e)
        {
            _ = _mainWindowViewModel.ResetTrackingNotificationsAsync();
        }

        private void BtnExportLootToFile_MouseUp(object sender, MouseEventArgs e)
        {
            _mainWindowViewModel.ExportLootToFile();
        }

        private void BtnTryToLoadTheItemListAgain_Click(object sender, RoutedEventArgs e)
        {
            _ = _mainWindowViewModel.InitItemListAsync().ConfigureAwait(false);
        }

        private void PartyIcon_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _ = _mainWindowViewModel.ResetPartyAsync();
        }

        private void OpenDamageMeterWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.OpenDamageMeterWindow();
        }

        private void CopyDamageMeterToClipboard_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.CopyDamageMeterToClipboard();
        }

        private void DamageMeterModeActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.DamageMeterActivationToggle();
        }
    }
}
