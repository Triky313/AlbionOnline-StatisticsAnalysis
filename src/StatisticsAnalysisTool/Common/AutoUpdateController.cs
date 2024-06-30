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
        var updateDirPath = Path.Combine(Environment.CurrentDirectory, Settings.Default.UpdatesDirectoryName);
        var executablePath = Path.Combine(Environment.CurrentDirectory, "StatisticsAnalysisTool.exe");
        string currentUpdateUrl = string.Empty;

        RemoveUpdateFiles(updateDirPath);

        if (SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive && await HttpClientUtils.IsUrlAccessible(Settings.Default.AutoUpdatePreReleaseConfigUrl))
        {
            currentUpdateUrl = Settings.Default.AutoUpdatePreReleaseConfigUrl;
        }
        else if (await HttpClientUtils.IsUrlAccessible(Settings.Default.AutoUpdateConfigUrl))
        {
            currentUpdateUrl = Settings.Default.AutoUpdateConfigUrl;
        }

        if (string.IsNullOrEmpty(currentUpdateUrl))
        {
            return;
        }

        try
        {
            AutoUpdater.Synchronous = true;
            AutoUpdater.ApplicationExitEvent -= AutoUpdaterApplicationExit;

            DirectoryController.CreateDirectoryWhenNotExists(updateDirPath);

            AutoUpdater.DownloadPath = updateDirPath;
            AutoUpdater.ExecutablePath = executablePath;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ReportErrors = reportErrors;
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.TopMost = true;

            AutoUpdater.Start(currentUpdateUrl);

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

    public static void RemoveUpdateFiles(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        try
        {
            foreach (var filePath in Directory.GetFiles(path, "StatisticsAnalysis-AlbionOnline-*-x64.zip"))
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