using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;

namespace StatisticsAnalysisTool.Common;

public class Server
{
    public static void SetServerLocationWithDialogAsync()
    {
        var dialogWindow = new ServerLocationSelectionWindow();
        var dialogResult = dialogWindow.ShowDialog();

        if (dialogResult is true)
        {
            var serverSelectionWindowViewModel = (ServerLocationSelectionWindowViewModel) dialogWindow.DataContext;
            var selectedLocationServer = serverSelectionWindowViewModel.SelectedServerLocation;

            SettingsController.CurrentSettings.ServerLocation = selectedLocationServer.ServerLocation;
        }
    }
}