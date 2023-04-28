using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using StatisticsAnalysisTool.ViewModels;

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
        var vm = (MainWindowViewModel)DataContext;
        vm?.ResetTrackingNotificationsAsync().ConfigureAwait(false);
    }

    private void BtnExportLootToFile_MouseUp(object sender, MouseEventArgs e)
    {
        var vm = (MainWindowViewModel)DataContext;
        vm?.ExportLootToFile();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
    }

    #endregion
}