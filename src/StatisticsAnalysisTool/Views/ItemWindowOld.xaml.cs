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
    public partial class ItemWindowOld
    {
        private readonly ItemWindowViewModelOld _itemWindowViewModelOld;

        public ItemWindowOld(Item item)
        {
            InitializeComponent();
            _itemWindowViewModelOld = new ItemWindowViewModelOld(this, item);
            DataContext = _itemWindowViewModelOld;
        }

        public void InitializeItemWindow(Item item) => _itemWindowViewModelOld.InitializeItemWindow(item);

        private void Hotbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _itemWindowViewModelOld.RunUpdate = false;
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
            _itemWindowViewModelOld.IsAutoUpdateActive = !_itemWindowViewModelOld.IsAutoUpdateActive;
            _itemWindowViewModelOld.RefreshSpin = _itemWindowViewModelOld.IsAutoUpdateActive;
        }

        private void FilterItemPriceValues()
        {
            _itemWindowViewModelOld.GetMainPriceStats();
            _itemWindowViewModelOld.SetQualityPriceStatsOnListView();
            _ = _itemWindowViewModelOld.SetHistoryChartPricesAsync();
            _ = _itemWindowViewModelOld.GetItemPricesInRealMoneyAsync();
        }
        
        private void LabelNotes_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is not TextBox textBox)
            {
                return;
            }

            CraftingTabController.AddNote(_itemWindowViewModelOld.Item.UniqueName, textBox.Text);
        }

        private void ItemWindow_OnClosing(object sender, CancelEventArgs e)
        {
            CraftingTabController.SaveInFile();
        }

        private void CraftingInfoPopup_MouseUp(object sender, MouseEventArgs e)
        {
            _itemWindowViewModelOld.CraftingInfoPopupVisibility = _itemWindowViewModelOld.CraftingInfoPopupVisibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }
    }
}