using System.ComponentModel;
using System.Windows.Controls;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Views
{
    using Models;
    using System.Windows;
    using System.Windows.Input;
    using ViewModels;

    /// <summary>
    ///     Interaktionslogik für ItemWindow.xaml
    /// </summary>
    public partial class ItemWindow
    {
        private readonly ItemWindowViewModel _itemWindowViewModel;

        public ItemWindow(Item item)
        {
            InitializeComponent();
            _itemWindowViewModel = new ItemWindowViewModel(this, item);
            DataContext = _itemWindowViewModel;
        }

        public void InitializeItemWindow(Item item) => _itemWindowViewModel.InitializeItemWindow(item);

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModel.RunUpdate = false;
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void ShowVillagesPrices_Click(object sender, RoutedEventArgs e)
        {
            FilterItemPriceValues();
        }

        private void ChbShowBlackZoneOutposts_Click(object sender, RoutedEventArgs e)
        {
            FilterItemPriceValues();
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

        private void CbNormalQuality_Click(object sender, RoutedEventArgs e)
        {
            FilterItemPriceValues();
        }
        private void CbGoodQuality_Click(object sender, RoutedEventArgs e)
        {
            FilterItemPriceValues();
        }
        private void CbCbOutstandingQuality_Click(object sender, RoutedEventArgs e)
        {
            FilterItemPriceValues();
        }
        private void CbExcellentQuality_Click(object sender, RoutedEventArgs e)
        {
            FilterItemPriceValues();
        }
        private void CbMasterpieceQuality_Click(object sender, RoutedEventArgs e)
        {
            FilterItemPriceValues();
        }

        private void ImageAwesome_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _itemWindowViewModel.IsAutoUpdateActive = !_itemWindowViewModel.IsAutoUpdateActive;
            _itemWindowViewModel.RefreshSpin = _itemWindowViewModel.IsAutoUpdateActive;
        }

        private void FilterItemPriceValues()
        {
            _itemWindowViewModel.GetMainPriceStats();
            _itemWindowViewModel.SetQualityPriceStatsOnListView();
            _ = _itemWindowViewModel.SetHistoryChartPricesAsync();
            _ = _itemWindowViewModel.GetItemPricesInRealMoneyAsync();
        }
        
        private void LabelNotes_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            CraftingTabController.Add(_itemWindowViewModel.Item.UniqueName, textBox.Text);
        }

        private void ItemWindow_OnClosing(object sender, CancelEventArgs e)
        {
            CraftingTabController.SaveInFile();
        }

        private void CraftingInfoPopup_MouseUp(object sender, MouseEventArgs e)
        {
            _itemWindowViewModel.CraftingInfoPopupVisibility = _itemWindowViewModel.CraftingInfoPopupVisibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }
    }
}