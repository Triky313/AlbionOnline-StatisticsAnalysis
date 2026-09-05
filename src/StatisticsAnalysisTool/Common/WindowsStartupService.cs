using Microsoft.Win32;
using Serilog;
using System;

namespace StatisticsAnalysisTool.Common;

public static class WindowsStartupService
{
    private const string RegistrySubKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ApplicationName = "AlbionOnlineStatisticsAnalysisTool";

    private static string RegistryValueName => $"{ApplicationName}.{AppInstance.InstanceId}";

    public static bool TrySetEnabled(bool isEnabled)
    {
        try
        {
            return isEnabled ? TryCreateRegistration() : TryRemoveRegistration();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "The Windows startup registration could not be updated");
            return false;
        }
    }

    private static bool TryCreateRegistration()
    {
        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
        {
            Log.Error("The Windows startup registration could not be created because the executable path is unavailable");
            return false;
        }

        using var registryKey = Registry.CurrentUser.CreateSubKey(RegistrySubKeyPath, true);
        if (registryKey is null)
        {
            Log.Error("The Windows startup registry key could not be opened");
            return false;
        }

        registryKey.SetValue(RegistryValueName, $"\"{executablePath}\"", RegistryValueKind.String);
        return true;
    }

    private static bool TryRemoveRegistration()
    {
        using var registryKey = Registry.CurrentUser.OpenSubKey(RegistrySubKeyPath, true);
        if (registryKey is null)
        {
            return true;
        }

        registryKey.DeleteValue(RegistryValueName, false);
        return true;
    }
}