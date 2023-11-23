using Microsoft.Extensions.DependencyInjection;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
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
        var dialog = new DialogWindow(LanguageController.Translation("RESET_TRACKING_NOTIFICATIONS"), LanguageController.Translation("SURE_YOU_WANT_TO_RESET_TRACKING_NOTIFICATIONS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            var mainWindowViewModel = App.ServiceProvider.GetRequiredService<MainWindowViewModelOld>();

            App.ServiceProvider.GetRequiredService<ILootController>()?.ClearLootLogger();
            mainWindowViewModel?.LoggingBindings?.TrackingNotifications.Clear();
            mainWindowViewModel?.LoggingBindings?.TopLooters?.Clear();
        }
    }

    private void BtnExportLootToFile_MouseUp(object sender, MouseEventArgs e)
    {
        App.ServiceProvider.GetRequiredService<MainWindowViewModelOld>()?.ExportLootToFile();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo { FileName = e.Uri.AbsoluteUri, UseShellExecute = true });
    }

    #endregion
}