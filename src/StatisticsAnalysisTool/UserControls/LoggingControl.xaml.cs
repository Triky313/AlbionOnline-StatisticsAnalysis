using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for LoggingControl.xaml
/// </summary>
public partial class LoggingControl
{
    public LoggingControl()
    {
        InitializeComponent();
    }

    #region Ui events

    private void BtnTrackingNotificationsReset_Click(object sender, RoutedEventArgs e)
    {
        var trackingController = ServiceLocator.Resolve<TrackingController>();
        trackingController?.ResetTrackingNotificationsAsync();
    }

    private void BtnExportLootToFile_MouseUp(object sender, MouseEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.ExportLootToFile();
    }

    private void BtnLoadVaultLogFiles_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.OpenVaultFilePathSelection();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private void BtnLogCompare_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.UpdateItemsStatus();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private void BtnClearVaultLogItems_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.VaultLogItems.Clear();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    private void BtnClearLootLogs_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = false;
        mainWindowViewModel.LoggingBindings.LootingPlayers.Clear();
        mainWindowViewModel.LoggingBindings.IsAllButtonsEnabled = true;
    }

    #endregion
}