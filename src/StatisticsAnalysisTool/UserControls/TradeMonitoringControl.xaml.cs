using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using StatisticsAnalysisTool.Trade.Mails;

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

    public void DeleteSelectedTrades()
    {
        var dialog = new DialogWindow(LanguageController.Translation("DELETE_SELECTED_MAILS"), LanguageController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_MAIL"));
        var dialogResult = dialog.ShowDialog();

        var vm = (MainWindowViewModel)DataContext;

        if (dialogResult is true)
        {
            // TODO: In TradeController einbinden und auf alle Trades anwenden.
            //var selectedMails = vm?.TradeMonitoringBindings?.Trade?.Where(x => x?.IsSelectedForDeletion ?? false).Select(x => x.Id);
            //vm?.TrackingController.MailController.RemoveMailsByIdsAsync(selectedMails);
        }
    }

    #region Ui events

    private void OpenMailMonitoringPopup_MouseEnter(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext;
        vm.TradeMonitoringBindings.IsMailMonitoringPopupVisible = Visibility.Visible;
    }

    private void CloseMailMonitoringPopup_MouseLeave(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext;
        vm.TradeMonitoringBindings.IsMailMonitoringPopupVisible = Visibility.Collapsed;
    }

    private void BtnDeleteSelectedMails_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedTrades();
    }

    private bool _isSelectAllActive;

    private void BtnSelectSwitchAllMails_Click(object sender, RoutedEventArgs e)
    {
        if ((MainWindowViewModel)DataContext is not { TradeMonitoringBindings.Trade: { } } mainWindowViewModel)
        {
            return;
        }

        foreach (var trade in mainWindowViewModel.TradeMonitoringBindings.Trade)
        {
            // TODO: Erweitern um InstantSell und Buy
            if (trade is Mail mail)
            {
                mail.IsSelectedForDeletion = !_isSelectAllActive;
            }
        }

        _isSelectAllActive = !_isSelectAllActive;
    }

    private void SearchText_TextChanged(object sender, TextChangedEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext;
        CollectionViewSource.GetDefaultView(vm.TradeMonitoringBindings.Trade).Refresh();
    }

    private void DatePicker_OnSelectedDateChanged(object sender, SelectionChangedEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext;
        CollectionViewSource.GetDefaultView(vm.TradeMonitoringBindings.Trade).Refresh();
    }

    #endregion
}