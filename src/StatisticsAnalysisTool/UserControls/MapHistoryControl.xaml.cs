using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Views;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.UserControls;

/// <summary>
/// Interaction logic for MapHistoryControl.xaml
/// </summary>
public partial class MapHistoryControl
{
    public MapHistoryControl()
    {
        InitializeComponent();
    }

    private async Task DeleteAllMapHistoryEntriesAsync()
    {
        var dialog = new DialogWindow(
            LocalizationController.Translation("DELETE_ALL_MAP_HISTORY_ENTRIES"),
            LocalizationController.Translation("SURE_YOU_WANT_TO_DELETE_ALL_MAP_HISTORY_ENTRIES"));
        var dialogResult = dialog.ShowDialog();

        if (dialogResult is not true)
        {
            return;
        }

        var trackingController = ServiceLocator.Resolve<TrackingController>();

        if (trackingController == null)
        {
            return;
        }

        await trackingController.ClusterController.ClearMapHistoryAsync();
    }

    private async void BtnDeleteAllMapHistoryEntries_Click(object sender, RoutedEventArgs e)
    {
        await DeleteAllMapHistoryEntriesAsync();
    }
}