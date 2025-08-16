using Serilog;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace StatisticsAnalysisTool.Common.UserSettings;

public class SettingsController
{
    public static SettingsObject CurrentSettings = new();

    private static bool _haveSettingsAlreadyBeenLoaded;

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
        SaveToLocalFile();
        ItemController.SaveFavoriteItemsToLocalFile();
    }

    public static void LoadSettings()
    {
        if (_haveSettingsAlreadyBeenLoaded)
        {
            return;
        }

        var localFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.SettingsFileName);

        try
        {
            if (!File.Exists(localFilePath))
            {
                CurrentSettings = new SettingsObject();
                SaveToLocalFile();
                _haveSettingsAlreadyBeenLoaded = true;
                return;
            }

            var settingsString = File.ReadAllText(localFilePath, Encoding.UTF8);
            var loaded = JsonSerializer.Deserialize<SettingsObject>(settingsString);

            CurrentSettings = loaded ?? new SettingsObject();
            _haveSettingsAlreadyBeenLoaded = true;
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);

            CurrentSettings ??= new SettingsObject();
            _haveSettingsAlreadyBeenLoaded = true;
        }
    }

    private static void SaveToLocalFile()
    {
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.SettingsFileName}";

        try
        {
            var fileString = JsonSerializer.Serialize(CurrentSettings);
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}