using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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

        var vm = (MainWindowViewModel) DataContext;

        if (dialogResult is true)
        {
            var selectedMails = vm?.TradeMonitoringBindings?.Trades?.Where(x => x?.IsSelectedForDeletion ?? false).Select(x => x.Id);
            ServiceLocator.Resolve<MainWindowViewModel>().TradeMonitoringBindings.IsDeleteTradesButtonEnabled = false;
            await vm?.TrackingController.TradeController.RemoveTradesByIdsAsync(selectedMails)!;
            ServiceLocator.Resolve<MainWindowViewModel>().TradeMonitoringBindings.IsDeleteTradesButtonEnabled = true;
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
    }

    private bool _isSelectAllActive;

    private void BtnSelectSwitchAllMails_Click(object sender, RoutedEventArgs e)
    {
        if ((MainWindowViewModel) DataContext is not { TradeMonitoringBindings.Trades: { } } mainWindowViewModel)
        {
            return;
        }

        foreach (var trade in mainWindowViewModel.TradeMonitoringBindings.Trades)
        {
            trade.IsSelectedForDeletion = !_isSelectAllActive;
        }

        _isSelectAllActive = !_isSelectAllActive;
    }

    private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        CollectionViewSource.GetDefaultView(vm.TradeMonitoringBindings.Trades).Refresh();
    }

    private void DatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        CollectionViewSource.GetDefaultView(vm.TradeMonitoringBindings.Trades).Refresh();
    }

    #endregion
}