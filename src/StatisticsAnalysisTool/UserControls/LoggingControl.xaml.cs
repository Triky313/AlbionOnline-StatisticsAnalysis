using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.StorageHistory;
using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

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
        mainWindowViewModel?.LoggingBindings?.OpenVaultFilePathSelection();
    }

    private void BtnLogCompare_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.LoggingBindings?.UpdateItemsStatus();
    }

    private void BtnClearVaultLogItems_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.LoggingBindings?.VaultLogItems.Clear();
    }

    private void BtnClearLootLogs_Click(object sender, RoutedEventArgs e)
    {
        var mainWindowViewModel = ServiceLocator.Resolve<MainWindowViewModel>();
        mainWindowViewModel?.LoggingBindings?.LootingPlayers.Clear();
    }

    #endregion
}