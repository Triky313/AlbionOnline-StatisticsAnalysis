using AutoUpdaterDotNET;
using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

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
            AutoUpdater.Proxy = null;
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
            AutoUpdater.CheckForUpdateEvent -= AutoUpdaterOnCheckForUpdateEvent;

            DirectoryController.CreateDirectoryWhenNotExists(updateDirPath);

            AutoUpdater.DownloadPath = updateDirPath;
            AutoUpdater.ExecutablePath = executablePath;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ReportErrors = reportErrors;
            AutoUpdater.ShowSkipButton = false;
            AutoUpdater.TopMost = true;

            if (reportErrors)
            {
                AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            }
            else
            {
                AutoUpdater.ApplicationExitEvent += AutoUpdaterApplicationExit;
            }

            AutoUpdater.Start(currentUpdateUrl);

            if (reportErrors)
            {
                AutoUpdater.CheckForUpdateEvent -= AutoUpdaterOnCheckForUpdateEvent;
            }
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

    private static void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
    {
        if (args.Error == null)
        {
            if (!args.IsUpdateAvailable)
            {
                _ = MessageBox.Show(LocalizationController.Translation("NO_UPDATE_AVAILABLE_MESSAGE"),
                    LocalizationController.Translation("NO_UPDATE_AVAILABLE_TITLE"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var isMandatoryUpdate = args.Mandatory?.Value == true;
            var message = isMandatoryUpdate
                ? string.Format(LocalizationController.Translation("UPDATE_AVAILABLE_REQUIRED_MESSAGE"), args.CurrentVersion, args.InstalledVersion)
                : string.Format(LocalizationController.Translation("UPDATE_AVAILABLE_OPTIONAL_MESSAGE"), args.CurrentVersion, args.InstalledVersion);
            var title = LocalizationController.Translation("UPDATE_AVAILABLE_TITLE");
            var button = isMandatoryUpdate ? MessageBoxButton.OK : MessageBoxButton.YesNo;
            var result = MessageBox.Show(message, title, button, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes || result == MessageBoxResult.OK)
            {
                StartDownload(args);
            }

            return;
        }

        if (args.Error is WebException)
        {
            _ = MessageBox.Show(LocalizationController.Translation("UPDATE_CHECK_FAILED_MESSAGE"),
                LocalizationController.Translation("UPDATE_CHECK_FAILED_TITLE"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _ = MessageBox.Show(args.Error.Message,
            args.Error.GetType().ToString(),
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private static void StartDownload(UpdateInfoEventArgs args)
    {
        try
        {
            if (AutoUpdater.DownloadUpdate(args))
            {
                AutoUpdaterApplicationExit();
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            _ = MessageBox.Show(e.Message,
                e.GetType().ToString(),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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