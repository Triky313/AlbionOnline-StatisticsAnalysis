using AutoUpdaterDotNET;
using log4net;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Application = System.Windows.Application;

namespace StatisticsAnalysisTool.Common;

public static class AutoUpdateController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public static void AutoUpdate(bool reportErrors = false)
    {
#pragma warning disable CA1416 // Validate platform compatibility

        AutoUpdater.ApplicationExitEvent -= AutoUpdaterApplicationExitAsync;

        AutoUpdater.Start(SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive
            ? Settings.Default.AutoUpdatePreReleaseConfigUrl
            : Settings.Default.AutoUpdateConfigUrl);

        AutoUpdater.DownloadPath = Environment.CurrentDirectory;
        AutoUpdater.RunUpdateAsAdmin = false;
        AutoUpdater.ReportErrors = reportErrors;
        AutoUpdater.ApplicationExitEvent += AutoUpdaterApplicationExitAsync;
#pragma warning restore CA1416 // Validate platform compatibility
    }

    private static async void AutoUpdaterApplicationExitAsync()
    {
        await Task.Delay(1000);
        Application.Current.Shutdown();
    }

    public static void RemoveUpdateFiles()
    {
        var localFilePath = AppDomain.CurrentDomain.BaseDirectory;

        try
        {
            foreach (var filePath in Directory.GetFiles(localFilePath, "StatisticsAnalysis-AlbionOnline-*-x64.zip"))
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
        }
        catch (Exception ex) when (ex is DirectoryNotFoundException or UnauthorizedAccessException or PathTooLongException)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Warn($"{MethodBase.GetCurrentMethod()?.DeclaringType}: {ex.Message}");
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error($"{MethodBase.GetCurrentMethod()?.DeclaringType}: {e.Message}");
        }
    }
}