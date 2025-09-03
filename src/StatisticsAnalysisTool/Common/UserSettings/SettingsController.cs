using Serilog;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Common.UserSettings;

public static class SettingsController
{
    public static SettingsObject CurrentSettings { get; private set; } = new();

    private static bool _haveSettingsAlreadyBeenLoaded;

    private static string SettingsFilePath =>
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SettingsFileName);

    public static void SetWindowSettings(WindowState windowState, double height, double width, double left, double top)
    {
        if (windowState != WindowState.Maximized)
        {
            CurrentSettings.MainWindowHeight = double.IsNegativeInfinity(height) || double.IsPositiveInfinity(height) ? 0 : height;
            CurrentSettings.MainWindowWidth = double.IsNegativeInfinity(width) || double.IsPositiveInfinity(width) ? 0 : width;
            CurrentSettings.MainWindowLeftPosition = left;
            CurrentSettings.MainWindowTopPosition = top;
        }

        CurrentSettings.MainWindowMaximized = windowState == WindowState.Maximized;
    }

    public static void SaveSettings()
    {
        try
        {
            var ok = FileController.SaveAsync(CurrentSettings, SettingsFilePath, ValidateSettings).GetAwaiter().GetResult();

            if (!ok)
            {
                Log.Warning("Settings save rejected or failed for {file}", SettingsFilePath);
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "SaveSettings failed for {file}", SettingsFilePath);
        }
    }

    public static async Task LoadSettingsAsync()
    {
        if (_haveSettingsAlreadyBeenLoaded)
        {
            return;
        }

        try
        {
            var loaded = await FileController.LoadAsync<SettingsObject>(SettingsFilePath, ValidateSettings) ?? new SettingsObject();

            CurrentSettings = loaded;
            _haveSettingsAlreadyBeenLoaded = true;

            if (File.Exists(SettingsFilePath) && !IsFileEmpty(SettingsFilePath))
            {
                return;
            }

            var ok = await FileController.SaveAsync(CurrentSettings, SettingsFilePath, ValidateSettings);

            if (!ok)
            {
                Log.Warning("Failed to write default settings to {file}", SettingsFilePath);
            }
        }
        catch (JsonException je)
        {
            Log.Warning(je, "Invalid JSON in settings file {file}", SettingsFilePath);

            CurrentSettings = new SettingsObject();
            _haveSettingsAlreadyBeenLoaded = true;

            TrySaveDefault();
        }
        catch (Exception e)
        {
            Log.Error(e, "LoadSettings failed for {file}", SettingsFilePath);

            CurrentSettings ??= new SettingsObject();
            _haveSettingsAlreadyBeenLoaded = true;

            TrySaveDefault();
        }
    }

    #region Helper

    private static bool ValidateSettings(SettingsObject s)
    {
        if (s is null) return false;

        if (Invalid(s.MainWindowHeight) || Invalid(s.MainWindowWidth) || Invalid(s.MainWindowLeftPosition) || Invalid(s.MainWindowTopPosition))
        {
            return false;
        }

        return true;

        bool Invalid(double v) => double.IsNaN(v) || double.IsInfinity(v);
    }

    private static bool IsFileEmpty(string path)
    {
        try
        {
            var info = new FileInfo(path);
            return !info.Exists || info.Length == 0;
        }
        catch
        {
            return true;
        }
    }

    private static void TrySaveDefault()
    {
        try
        {
            var ok = FileController.SaveAsync(CurrentSettings, SettingsFilePath, ValidateSettings).GetAwaiter().GetResult();
            if (!ok)
            {
                Log.Warning("Could not persist default settings to {file}", SettingsFilePath);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while persisting default settings to {file}", SettingsFilePath);
        }
    }

    #endregion
}
