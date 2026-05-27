using Serilog;
using StatisticsAnalysisTool.Enumerations;
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

    private static string SettingsFilePath => AppDataPaths.SettingsFile;

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
        SaveSettingsAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task SaveSettingsAsync()
    {
        try
        {
            var ok = await FileController.SaveAsync(CurrentSettings, SettingsFilePath, ValidateSettings).ConfigureAwait(false);

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
            var loaded = await FileController.LoadAsync<SettingsObject>(SettingsFilePath, ValidateSettings).ConfigureAwait(false) ?? new SettingsObject();

            CurrentSettings = loaded;
            NormalizeRuntimePaths();
            MigrateLegacyUserDataIfNeeded();
            _haveSettingsAlreadyBeenLoaded = true;

            if (File.Exists(SettingsFilePath) && !IsFileEmpty(SettingsFilePath))
            {
                return;
            }

            var ok = await FileController.SaveAsync(CurrentSettings, SettingsFilePath, ValidateSettings).ConfigureAwait(false);

            if (!ok)
            {
                Log.Warning("Failed to write default settings to {file}", SettingsFilePath);
            }
        }
        catch (JsonException je)
        {
            Log.Warning(je, "Invalid JSON in settings file {file}", SettingsFilePath);

            CurrentSettings = new SettingsObject();
            NormalizeRuntimePaths();
            MigrateLegacyUserDataIfNeeded();
            _haveSettingsAlreadyBeenLoaded = true;

            TrySaveDefault();
        }
        catch (Exception e)
        {
            Log.Error(e, "LoadSettings failed for {file}", SettingsFilePath);

            CurrentSettings ??= new SettingsObject();
            NormalizeRuntimePaths();
            MigrateLegacyUserDataIfNeeded();
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

    private static void NormalizeRuntimePaths()
    {
        CurrentSettings.BackupStorageDirectoryPath = AppDataPaths.BackupsDirectory;
    }

    private static void MigrateLegacyUserDataIfNeeded()
    {
        if (!TryReadLegacyServerLocation(out var serverLocation))
        {
            return;
        }

        var hasMigrated = AppDataMigration.TryMigrateLegacyUserDataToServerDirectory(serverLocation, out var migrationMessages);
        AppDataMigration.LogMessages(migrationMessages);

        if (hasMigrated)
        {
            _ = FileController.SaveAsync(CurrentSettings, SettingsFilePath, ValidateSettings).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

    private static bool TryReadLegacyServerLocation(out ServerLocation serverLocation)
    {
        serverLocation = ServerLocation.Unknown;

        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                return false;
            }

            using var document = JsonDocument.Parse(File.ReadAllText(SettingsFilePath));
            if (!document.RootElement.TryGetProperty("ServerLocation", out var serverLocationElement))
            {
                return false;
            }

            serverLocation = serverLocationElement.ValueKind switch
            {
                JsonValueKind.Number when serverLocationElement.TryGetInt32(out var value) => ToServerLocation(value),
                JsonValueKind.String => ToServerLocation(serverLocationElement.GetString()),
                _ => ServerLocation.Unknown
            };

            return serverLocation is ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Legacy ServerLocation could not be read from settings.");
            return false;
        }
    }

    private static ServerLocation ToServerLocation(int value)
    {
        return Enum.IsDefined(typeof(ServerLocation), value)
            ? (ServerLocation) value
            : ServerLocation.Unknown;
    }

    private static ServerLocation ToServerLocation(string value)
    {
        if (Enum.TryParse<ServerLocation>(value, true, out var serverLocation))
        {
            return serverLocation;
        }

        return value?.ToUpperInvariant() switch
        {
            "AMERICA" or "AMERICAS" or "US" or "WEST" => ServerLocation.America,
            "ASIA" or "EAST" => ServerLocation.Asia,
            "EUROPE" or "EU" => ServerLocation.Europe,
            _ => ServerLocation.Unknown
        };
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
            var ok = FileController.SaveAsync(CurrentSettings, SettingsFilePath, ValidateSettings).ConfigureAwait(false).GetAwaiter().GetResult();
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
