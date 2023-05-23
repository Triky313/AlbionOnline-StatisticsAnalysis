using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Diagnostics;
using System.Windows;
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

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
    }

    #endregion
}