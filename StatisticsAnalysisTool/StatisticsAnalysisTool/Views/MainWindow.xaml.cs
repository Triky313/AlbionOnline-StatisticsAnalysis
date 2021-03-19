﻿using FontAwesome.WPF;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network;
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
        private static bool _isWindowMaximized;

        public MainWindow()
        {
            InitializeComponent();
            _mainWindowViewModel = new MainWindowViewModel(this);
            DataContext = _mainWindowViewModel;
        }

        private void LvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (Item) ((ListView) sender).SelectedValue;

            MainWindowViewModel.OpenItemWindow(item);
        }

        private void ImageAwesome_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var sw = new SettingsWindow(_mainWindowViewModel);
            sw.ShowDialog();
        }

        private void StopLoadFullItemInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _mainWindowViewModel.IsFullItemInfoLoading = false;
        }

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkManager.StopNetworkCapture();
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
                MaximizedButton.Content = 2;
                _isWindowMaximized = true;
                return;
            }

            if (e.ClickCount == 2 && WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                _mainWindowViewModel.CenterWindowOnScreen();
                MaximizedButton.Content = 1;
                _isWindowMaximized = false;
            }
        }

        private void MaximizedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isWindowMaximized)
            {
                WindowState = WindowState.Normal;
                _mainWindowViewModel.CenterWindowOnScreen();
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

        private void CbMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _mainWindowViewModel.SelectViewModeGrid();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
        }
        
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            #region Tracking

            if (_mainWindowViewModel != null)
            {
                Settings.Default.IsTrackingResetByMapChangeActive = _mainWindowViewModel.IsTrackingResetByMapChangeActive;
                Settings.Default.IsTrackingActiveAtToolStart = _mainWindowViewModel.IsTrackingActive;

                if (_mainWindowViewModel.IsTrackingActive)
                {
                    _mainWindowViewModel.StopTracking();
                }
            }

            #endregion

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
            
            Settings.Default.Save();

            ItemController.SaveItemInformationLocal();
        }

        private async void BtnPlayerModeSave_Click(object sender, RoutedEventArgs e)
        {
            await _mainWindowViewModel.SetComparedPlayerModeInfoValues();
        }

        private async void TxtBoxPlayerModeUsername_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            await _mainWindowViewModel.SetComparedPlayerModeInfoValues();
        }

        private void TxtBoxGoldModeAmountValues_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
        }

        private void TxtBoxMinSellPriceIsUndercutPrice_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(_mainWindowViewModel.TextBoxGoldModeNumberOfValues, out var numberOfValues))
                _mainWindowViewModel.SetGoldChart(numberOfValues);
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
            _mainWindowViewModel.ResetMainCounters(true, true, true, true);
        }

        private void BtnDungeonTrackingReset_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.ResetDungeonCounters();
            _mainWindowViewModel.TrackingDungeons.Clear();
        }

        private void BtnErrorBar_Click(object sender, RoutedEventArgs e)
        {
            _mainWindowViewModel.ErrorBarVisibility = Visibility.Hidden;
            _mainWindowViewModel.ErrorBarText = string.Empty;
        }
    }
}