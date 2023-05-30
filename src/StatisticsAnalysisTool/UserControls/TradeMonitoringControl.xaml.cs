using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for TradeMonitoringControl.xaml
/// </summary>
public partial class TradeMonitoringControl
{
    public TradeMonitoringControl()
    {
        InitializeComponent();
    }

    public async Task DeleteSelectedTradesAsync()
    {
        var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_TRADES"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_TRADES"));
        var dialogResult = dialog.ShowDialog();

        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();

        if (mainWindowViewModel == null)
        {
            return;
        }

        if (dialogResult is true)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();

            var selectedTrades = mainWindowViewModel.TradeMonitoringBindings.TradeCollectionView?.Cast<Trade.Trade>()
                .ToList()
                .Where(x => x?.IsSelectedForDeletion ?? false)
                .Select(x => x.Id);

            mainWindowViewModel.TradeMonitoringBindings.IsDeleteTradesButtonEnabled = false;
            await trackingController?.TradeController?.RemoveTradesByIdsAsync(selectedTrades)!;
            mainWindowViewModel.TradeMonitoringBindings.IsDeleteTradesButtonEnabled = true;
        }
    }

    #region Ui events

    private void OpenMailMonitoringPopup_MouseEnter(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.TradeMonitoringBindings.IsTradeMonitoringPopupVisible = Visibility.Visible;
    }

    private void CloseMailMonitoringPopup_MouseLeave(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.TradeMonitoringBindings.IsTradeMonitoringPopupVisible = Visibility.Collapsed;
    }

    private async void BtnDeleteSelectedMails_Click(object sender, RoutedEventArgs e)
    {
        await DeleteSelectedTradesAsync();
        _isSelectAllActive = !_isSelectAllActive;
    }

    private bool _isSelectAllActive;

    private void BtnSelectSwitchAllMails_Click(object sender, RoutedEventArgs e)
    {
        if ((MainWindowViewModel) DataContext is not { TradeMonitoringBindings.Trades: { } } mainWindowViewModel)
        {
            return;
        }

        foreach (var trade in mainWindowViewModel.TradeMonitoringBindings.TradeCollectionView)
        {
            ((Trade.Trade) trade).IsSelectedForDeletion = !_isSelectAllActive;
        }

        _isSelectAllActive = !_isSelectAllActive;
    }

    private async void SearchText_TextChanged(object sender, TextChangedEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        await vm.TradeMonitoringBindings.UpdateFilteredTradesAsync();
    }

    private async void DatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        await vm.TradeMonitoringBindings.UpdateFilteredTradesAsync();
    }

    #endregion
}