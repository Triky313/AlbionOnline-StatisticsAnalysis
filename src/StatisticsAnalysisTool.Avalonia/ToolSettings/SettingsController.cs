using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Avalonia.Controls;
using Serilog;

namespace StatisticsAnalysisTool.Avalonia.ToolSettings;

public class SettingsController : ISettingsController
{
    public static UserSettings CurrentUserSettings = new();

    private static bool _haveSettingsAlreadyBeenLoaded;

    public void SetWindowSettings(WindowState windowState, double height, double width, double left, double top)
    {
        if (windowState != WindowState.Maximized)
        {
            CurrentUserSettings.MainWindowHeight = double.IsNegativeInfinity(height) || double.IsPositiveInfinity(height) ? 0 : height;
            CurrentUserSettings.MainWindowWidth = double.IsNegativeInfinity(width) || double.IsPositiveInfinity(width) ? 0 : width;
            CurrentUserSettings.MainWindowLeftPosition = left;
            CurrentUserSettings.MainWindowTopPosition = top;
        }

        CurrentUserSettings.MainWindowMaximized = windowState == WindowState.Maximized;
    }

    public void SaveSettings()
    {
        SaveToLocalFile();
        // TODO: Aktivieren, wenn Klasse vorhanden ist
        //ItemController.SaveFavoriteItemsToLocalFile();
    }

    public void LoadSettings()
    {
        if (_haveSettingsAlreadyBeenLoaded)
        {
            return;
        }

        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{AppSettings.SettingsFileName}";

        if (File.Exists(localFilePath))
        {
            try
            {
                var settingsString = File.ReadAllText(localFilePath, Encoding.UTF8);
                CurrentUserSettings = JsonSerializer.Deserialize<UserSettings>(settingsString);
                _haveSettingsAlreadyBeenLoaded = true;
            }
            catch (Exception e)
            {
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    private static void SaveToLocalFile()
    {
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{AppSettings.SettingsFileName}";

        try
        {
            var fileString = JsonSerializer.Serialize(CurrentUserSettings);
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}