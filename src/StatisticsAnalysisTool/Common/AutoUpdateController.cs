using AutoUpdaterDotNET;
using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Diagnostics;
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

        try
        {
            var isUrlAccessibleResult = await HttpClientUtils.IsUrlAccessible(Settings.Default.AutoUpdatePreReleaseConfigUrl);

            if (SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive && isUrlAccessibleResult is { IsAccessible: true, IsProxyActive: true })
            {
                AutoUpdater.Proxy = new WebProxy(SettingsController.CurrentSettings.ProxyUrlWithPort);
                currentUpdateUrl = Settings.Default.AutoUpdatePreReleaseConfigUrl;
            }
            else if (SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive && isUrlAccessibleResult is { IsAccessible: true, IsProxyActive: false })
            {
                currentUpdateUrl = Settings.Default.AutoUpdatePreReleaseConfigUrl;
            }
            else if (isUrlAccessibleResult is { IsAccessible: true, IsProxyActive: true })
            {
                AutoUpdater.Proxy = new WebProxy(SettingsController.CurrentSettings.ProxyUrlWithPort);
                currentUpdateUrl = Settings.Default.AutoUpdateConfigUrl;
            }
            else if (isUrlAccessibleResult is { IsAccessible: true, IsProxyActive: false })
            {
                currentUpdateUrl = Settings.Default.AutoUpdateConfigUrl;
            }

            if (string.IsNullOrEmpty(currentUpdateUrl))
            {
                return;
            }

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
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Warning(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
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
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Warning(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}