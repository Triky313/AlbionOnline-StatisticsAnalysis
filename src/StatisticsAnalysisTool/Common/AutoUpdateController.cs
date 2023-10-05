using AutoUpdaterDotNET;
using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Application = System.Windows.Application;

namespace StatisticsAnalysisTool.Common;

public static class AutoUpdateController
{
    public static async Task AutoUpdateAsync(bool reportErrors = false)
    {
        try
        {
            await HttpClientUtils.IsUrlAccessible(Settings.Default.AutoUpdatePreReleaseConfigUrl);
            await HttpClientUtils.IsUrlAccessible(Settings.Default.AutoUpdateConfigUrl);

            AutoUpdater.Synchronous = true;
            AutoUpdater.ApplicationExitEvent -= AutoUpdaterApplicationExit;

            AutoUpdater.Start(SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive
                ? Settings.Default.AutoUpdatePreReleaseConfigUrl
                : Settings.Default.AutoUpdateConfigUrl);

            AutoUpdater.DownloadPath = Environment.CurrentDirectory;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ReportErrors = reportErrors;
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.TopMost = true;
            AutoUpdater.ApplicationExitEvent += AutoUpdaterApplicationExit;
        }
        catch (HttpRequestException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static void AutoUpdaterApplicationExit()
    {
        AutoUpdater.ApplicationExitEvent -= AutoUpdaterApplicationExit;
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
            Log.Warning(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}