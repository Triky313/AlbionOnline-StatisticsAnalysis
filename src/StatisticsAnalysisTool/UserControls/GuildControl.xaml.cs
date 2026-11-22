using System.Linq;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for LoggingControl.xaml
/// </summary>
public partial class GuildControl
{
    private bool _isSelectAllActive;

    public GuildControl()
    {
        InitializeComponent();
    }

    private async void BtnDeleteSelectedSiphonedEnergyEntries_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new DialogWindow(LocalizationController.Translation("DELETE_SELECTED_ENTRIES"), LocalizationController.Translation("SURE_YOU_WANT_TO_DELETE_SELECTED_ENTRIES"));
        var dialogResult = dialog.ShowDialog();

        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();

        if (mainWindowViewModel == null)
        {
            return;
        }

        if (dialogResult is true)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();

            var selectedEntries = mainWindowViewModel.GuildBindings.SiphonedEnergyList
                .Where(x => x?.IsSelectedForDeletion ?? false)
                .Select(x => x.GetHashCode())
                .ToList();

            mainWindowViewModel.GuildBindings.IsDeleteEntriesButtonEnabled = false;
            await trackingController?.GuildController?.RemoveTradesByIdsAsync(selectedEntries)!;
            mainWindowViewModel.GuildBindings.IsDeleteEntriesButtonEnabled = true;
        }
    }

    private void ResetOverviewNameFilter_Click(object sender, RoutedEventArgs e)
    {
        OverviewNameFilter.Clear();
    }

    private void ResetDetailsNameFilter_Click(object sender, RoutedEventArgs e)
    {
        DetailsNameFilter.Clear();
    }

    private void OpenSiphonedEnergyInfoPopup_MouseEnter(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.GuildBindings.GuildPopupVisibility = Visibility.Visible;
    }

    private void CloseSiphonedEnergyInfoPopup_MouseLeave(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel) DataContext;
        vm.GuildBindings.GuildPopupVisibility = Visibility.Collapsed;
    }

    private void BtnSelectSwitchAllSiphonedEnergyEntries_Click(object sender, RoutedEventArgs e)
    {
        if ((MainWindowViewModel) DataContext is not { GuildBindings.SiphonedEnergyList: not null } mainWindowViewModel)
        {
            return;
        }

        foreach (var item in mainWindowViewModel.GuildBindings.SiphonedEnergyList)
        {
            item.IsSelectedForDeletion = !_isSelectAllActive;
        }

        _isSelectAllActive = !_isSelectAllActive;
    }
}
