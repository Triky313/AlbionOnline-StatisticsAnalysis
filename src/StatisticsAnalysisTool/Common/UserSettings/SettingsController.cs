using log4net;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace StatisticsAnalysisTool.Common.UserSettings;

public static class SettingsController
{
    public static SettingsObject CurrentSettings = new();

    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static bool _haveSettingsAlreadyBeenLoaded;

    public static void SaveSettings(WindowState windowState, double height, double width)
    {
        #region Window

        if (windowState != WindowState.Maximized)
        {
            CurrentSettings.MainWindowHeight = double.IsNegativeInfinity(height) || double.IsPositiveInfinity(height) ? 0 : height;
            CurrentSettings.MainWindowWidth = double.IsNegativeInfinity(width) || double.IsPositiveInfinity(width) ? 0 : width;
        }

        CurrentSettings.MainWindowMaximized = windowState == WindowState.Maximized;

        #endregion

        SaveToLocalFile();

        ItemController.SaveFavoriteItemsToLocalFile();
    }

    public static void LoadSettings()
    {
        if (_haveSettingsAlreadyBeenLoaded)
        {
            return;
        }

        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.SettingsFileName}";

        if (File.Exists(localFilePath))
        {
            try
            {
                var settingsString = File.ReadAllText(localFilePath, Encoding.UTF8);
                CurrentSettings = JsonSerializer.Deserialize<SettingsObject>(settingsString);
                _haveSettingsAlreadyBeenLoaded = true;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
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
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}