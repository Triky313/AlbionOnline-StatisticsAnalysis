using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Views;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

public partial class OpenWorldControl
{
    public OpenWorldControl()
    {
        InitializeComponent();
    }

    private async void BtnResetOpenWorldStats_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new DialogWindow(LocalizationController.Translation("RESET_OPEN_WORLD_STATS"), LocalizationController.Translation("SURE_YOU_WANT_TO_RESET_OPEN_WORLD_STATS"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is true)
        {
            var trackingController = ServiceLocator.Resolve<TrackingController>();
            await trackingController.OpenWorldController.ResetStatsAsync();
        }
    }
}