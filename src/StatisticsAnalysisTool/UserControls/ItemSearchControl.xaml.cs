using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.UserControls
{
    /// <summary>
    /// Interaction logic for ItemSearchControl.xaml
    /// </summary>
    public partial class ItemSearchControl : UserControl
    {
        private readonly ItemSearchViewModel _viewModel;

        public ItemSearchControl()
        {
            InitializeComponent();
            _viewModel = new ItemSearchViewModel(this);
            DataContext = _viewModel;
        }

        private void LvItems_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (Item)((ListView)sender).SelectedValue;

            MainWindowViewModel.OpenItemWindow(item);
        }

        private void FilterReset_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _viewModel.ItemFilterReset();
        }

        private void BtnTryToLoadTheItemListAgain_Click(object sender, RoutedEventArgs e)
        {
            _ = _viewModel.InitItemListAsync().ConfigureAwait(false);
        }

        private void AlertModeAlertActiveToggle_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _viewModel.ToggleAlertSender(sender);
        }

        private void StopLoadFullItemInfo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _viewModel.IsFullItemInfoLoading = false;
        }

        private void LoadFullItemInfoButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadAllFullItemInformationFromWeb();
        }

        private void TxtBoxMinSellPriceIsUndercutPrice_OnPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text.Last()) && e.Text.Last() != '.';
        }

        public string ItemList
        {
            get => (string)GetValue(ErrorBarTextProperty);
            set => SetValue(ErrorBarTextProperty, value);
        }

        public static readonly DependencyProperty ErrorBarTextProperty = DependencyProperty.Register("ItemList", typeof(string), typeof(ItemSearchControl));
    }
}
