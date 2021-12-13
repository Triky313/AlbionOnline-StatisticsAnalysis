using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for ItemSearchControl.xaml
    /// </summary>
    public partial class ItemSearchControl
    {
        public ItemSearchControl()
        {
            InitializeComponent();
        }

        #region Ui events

        private void LvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (Item)((ListView)sender).SelectedValue;
            MainWindowViewModel.OpenItemWindow(item);
        }

        private void LoadFullItemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.LoadAllFullItemInformationFromWeb();
        }

        private void FilterReset_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.ItemFilterReset();
        }

        private void StopLoadFullItemInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm.IsFullItemInfoLoading = false;
        }

        private void TxtBoxMinSellPriceIsUndercutPrice_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
        }

        private void AlertModeAlertActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.ToggleAlertSender(sender);
        }

        private void BtnTryToLoadTheItemListAgain_Click(object sender, RoutedEventArgs e)
        {
            var vm = (MainWindowViewModel)DataContext;
            vm?.InitItemListAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
