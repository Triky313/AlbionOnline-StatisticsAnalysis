using System;
using System.Threading.Tasks;
using System.Windows;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Common;

public static class AutoUpdateController
{
    public static void AutoUpdate()
    {
        if (SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            AutoUpdater.Start(Settings.Default.AutoUpdatePreReleaseConfigUrl);
            AutoUpdater.DownloadPath = Environment.CurrentDirectory;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ApplicationExitEvent += AutoUpdaterApplicationExitAsync;
#pragma warning restore CA1416 // Validate platform compatibility
        }
        else
        {
#pragma warning disable CA1416 // Validate platform compatibility
            AutoUpdater.Start(Settings.Default.AutoUpdateConfigUrl);
            AutoUpdater.DownloadPath = Environment.CurrentDirectory;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ApplicationExitEvent += AutoUpdaterApplicationExitAsync;
#pragma warning restore CA1416 // Validate platform compatibility
        }
    }

    private static async void AutoUpdaterApplicationExitAsync()
    {
        await Task.Delay(3000);
        Application.Current.Shutdown();
    }
}