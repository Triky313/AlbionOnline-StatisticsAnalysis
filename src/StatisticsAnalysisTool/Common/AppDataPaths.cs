using System;
using System.Collections.Generic;
using System.IO;

namespace StatisticsAnalysisTool.Common;

public static class AppDataPaths
{
    private const string AppDataFolderName = "StatisticsAnalysisTool";
    private const string InstancesDirectoryName = "Instances";
    private const string LegacyDefaultDirectoryName = "Default";
    private const string BackupsDirectoryName = "Backups";
    private const string UserDataDirectoryName = "UserData";
    private const string TempDirectoryName = "temp";
    private const string SpellImageResourcesDirectoryName = "SpellImageResources";
    private const string LogsDirectoryName = "logs";
    private const string ImageResourcesDirectoryName = "ImageResources";
    private const string GameFilesDirectoryName = "GameFiles";
    private const string SettingsFileName = "Settings.json";
    private const string LogFilePatternName = "sat-.logs";
    private const string SoundDirectoryName = "Sounds";
    private const string LocalizationDirectoryName = "Localization";
    private const string LocalizationFileName = "localization.json";
    private const string ExecutableFileName = "StatisticsAnalysisTool.exe";
    private static string _runtimeBaseDirectoryOverride;
    private static string _installationDirectoryOverride;
    private static string _legacyDefaultDirectoryOverride;

    public static string BaseDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppDataFolderName);

    public static string InstancesDirectory => Path.Combine(BaseDirectory, InstancesDirectoryName);

    public static string LegacyDefaultDirectory => _legacyDefaultDirectoryOverride ?? Path.Combine(BaseDirectory, LegacyDefaultDirectoryName);

    public static string Root => Path.Combine(
        InstancesDirectory,
        AppInstance.InstanceId);

    public static string RuntimeBaseDirectory => _runtimeBaseDirectoryOverride ?? Root;

    public static string InstallationDirectory => _installationDirectoryOverride ?? AppContext.BaseDirectory;

    public static string Backups => Path.Combine(RuntimeBaseDirectory, BackupsDirectoryName);

    public static string UserData => Path.Combine(RuntimeBaseDirectory, UserDataDirectoryName);

    public static string Temp => Path.Combine(RuntimeBaseDirectory, TempDirectoryName);

    public static string SpellImageResources => Path.Combine(RuntimeBaseDirectory, SpellImageResourcesDirectoryName);

    public static string Logs => Path.Combine(RuntimeBaseDirectory, LogsDirectoryName);

    public static string ImageResources => Path.Combine(RuntimeBaseDirectory, ImageResourcesDirectoryName);

    public static string GameFiles => Path.Combine(RuntimeBaseDirectory, GameFilesDirectoryName);

    public static string SettingsFile => Path.Combine(RuntimeBaseDirectory, SettingsFileName);

    public static string BackupsDirectory => Backups;

    public static string UserDataDirectory => UserData;

    public static string TempDirectory => Temp;

    public static string SpellImageResourcesDirectory => SpellImageResources;

    public static string LogsDirectory => Logs;

    public static string ImageResourcesDirectory => ImageResources;

    public static string GameFilesDirectory => GameFiles;

    public static string LogFilePattern => Path.Combine(LogsDirectory, LogFilePatternName);

    public static string SoundDirectory => InstallationFile(SoundDirectoryName);

    public static string SoundFile(string fileName)
    {
        return Path.Combine(SoundDirectory, fileName);
    }

    public static string LocalizationFile => InstallationFile(LocalizationDirectoryName, LocalizationFileName);
    public static string ExecutableFile => InstallationFile(ExecutableFileName);

    public static IReadOnlyCollection<string> RuntimeDirectories =>
    [
        BackupsDirectory,
        UserDataDirectory,
        TempDirectory,
        SpellImageResourcesDirectory,
        LogsDirectory,
        ImageResourcesDirectory,
        GameFilesDirectory
    ];

    public static string UserDataFile(string fileName)
    {
        return Path.Combine(UserDataDirectory, fileName);
    }

    public static string GameFile(string fileName)
    {
        return Path.Combine(GameFilesDirectory, fileName);
    }

    public static string ItemImageFile(string fileName)
    {
        return Path.Combine(ImageResourcesDirectory, fileName);
    }

    public static string SpellImageFile(string fileName)
    {
        return Path.Combine(SpellImageResourcesDirectory, fileName);
    }

    public static string TempFile(string fileName)
    {
        return Path.Combine(TempDirectory, fileName);
    }

    public static string InstallationFile(params string[] pathParts)
    {
        return Path.Combine([InstallationDirectory, .. pathParts]);
    }

    public static string LegacyBackupsDirectory => Path.Combine(InstallationDirectory, BackupsDirectoryName);

    public static string LegacyUserDataDirectory => Path.Combine(InstallationDirectory, UserDataDirectoryName);

    public static string LegacyTempDirectory => Path.Combine(InstallationDirectory, TempDirectoryName);

    public static string LegacySpellImageResourcesDirectory => Path.Combine(InstallationDirectory, SpellImageResourcesDirectoryName);

    public static string LegacyLogsDirectory => Path.Combine(InstallationDirectory, LogsDirectoryName);

    public static string LegacyImageResourcesDirectory => Path.Combine(InstallationDirectory, ImageResourcesDirectoryName);

    public static string LegacyGameFilesDirectory => Path.Combine(InstallationDirectory, GameFilesDirectoryName);

    public static string LegacySettingsFile => Path.Combine(InstallationDirectory, SettingsFileName);

    public static string LegacyDefaultBackupsDirectory => Path.Combine(LegacyDefaultDirectory, BackupsDirectoryName);

    public static string LegacyDefaultUserDataDirectory => Path.Combine(LegacyDefaultDirectory, UserDataDirectoryName);

    public static string LegacyDefaultTempDirectory => Path.Combine(LegacyDefaultDirectory, TempDirectoryName);

    public static string LegacyDefaultSpellImageResourcesDirectory => Path.Combine(LegacyDefaultDirectory, SpellImageResourcesDirectoryName);

    public static string LegacyDefaultLogsDirectory => Path.Combine(LegacyDefaultDirectory, LogsDirectoryName);

    public static string LegacyDefaultImageResourcesDirectory => Path.Combine(LegacyDefaultDirectory, ImageResourcesDirectoryName);

    public static string LegacyDefaultGameFilesDirectory => Path.Combine(LegacyDefaultDirectory, GameFilesDirectoryName);

    public static string LegacyDefaultSettingsFile => Path.Combine(LegacyDefaultDirectory, SettingsFileName);

    internal static IDisposable UseRuntimeBaseDirectoryForTests(string runtimeBaseDirectory)
    {
        var previousRuntimeBaseDirectoryOverride = _runtimeBaseDirectoryOverride;
        _runtimeBaseDirectoryOverride = runtimeBaseDirectory;
        return new RuntimeBaseDirectoryOverrideScope(previousRuntimeBaseDirectoryOverride);
    }

    internal static IDisposable UsePathOverridesForTests(
        string runtimeBaseDirectory,
        string installationDirectory,
        string legacyDefaultDirectory)
    {
        var previousRuntimeBaseDirectoryOverride = _runtimeBaseDirectoryOverride;
        var previousInstallationDirectoryOverride = _installationDirectoryOverride;
        var previousLegacyDefaultDirectoryOverride = _legacyDefaultDirectoryOverride;

        _runtimeBaseDirectoryOverride = runtimeBaseDirectory;
        _installationDirectoryOverride = installationDirectory;
        _legacyDefaultDirectoryOverride = legacyDefaultDirectory;

        return new PathOverrideScope(
            previousRuntimeBaseDirectoryOverride,
            previousInstallationDirectoryOverride,
            previousLegacyDefaultDirectoryOverride);
    }

    public static void EnsureBaseDirectory()
    {
        Directory.CreateDirectory(RuntimeBaseDirectory);
    }

    public static void EnsureRuntimeDirectories()
    {
        EnsureBaseDirectory();

        foreach (var runtimeDirectory in RuntimeDirectories)
        {
            Directory.CreateDirectory(runtimeDirectory);
        }
    }

    private sealed class RuntimeBaseDirectoryOverrideScope : IDisposable
    {
        private readonly string _previousRuntimeBaseDirectoryOverride;
        private bool _isDisposed;

        public RuntimeBaseDirectoryOverrideScope(string previousRuntimeBaseDirectoryOverride)
        {
            _previousRuntimeBaseDirectoryOverride = previousRuntimeBaseDirectoryOverride;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _runtimeBaseDirectoryOverride = _previousRuntimeBaseDirectoryOverride;
            _isDisposed = true;
        }
    }

    private sealed class PathOverrideScope : IDisposable
    {
        private readonly string _previousRuntimeBaseDirectoryOverride;
        private readonly string _previousInstallationDirectoryOverride;
        private readonly string _previousLegacyDefaultDirectoryOverride;
        private bool _isDisposed;

        public PathOverrideScope(
            string previousRuntimeBaseDirectoryOverride,
            string previousInstallationDirectoryOverride,
            string previousLegacyDefaultDirectoryOverride)
        {
            _previousRuntimeBaseDirectoryOverride = previousRuntimeBaseDirectoryOverride;
            _previousInstallationDirectoryOverride = previousInstallationDirectoryOverride;
            _previousLegacyDefaultDirectoryOverride = previousLegacyDefaultDirectoryOverride;
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _runtimeBaseDirectoryOverride = _previousRuntimeBaseDirectoryOverride;
            _installationDirectoryOverride = _previousInstallationDirectoryOverride;
            _legacyDefaultDirectoryOverride = _previousLegacyDefaultDirectoryOverride;
            _isDisposed = true;
        }
    }
}
