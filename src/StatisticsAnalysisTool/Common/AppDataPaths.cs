using System;
using System.Collections.Generic;
using System.IO;

namespace StatisticsAnalysisTool.Common;

public static class AppDataPaths
{
    private const string AppDataFolderName = "StatisticsAnalysisTool";
    private const string DebugDirectoryName = "Debug";
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

    public static string BaseDirectory { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), AppDataFolderName);

    public static string BaseDebugDirectory { get; } = Path.Combine(BaseDirectory, DebugDirectoryName);

    public static string RuntimeBaseDirectory => _runtimeBaseDirectoryOverride ?? (IsDebugBuild ? BaseDebugDirectory : BaseDirectory);

    public static string InstallationDirectory => AppDomain.CurrentDomain.BaseDirectory;

    public static string BackupsDirectory => Path.Combine(RuntimeBaseDirectory, BackupsDirectoryName);

    public static string UserDataDirectory => Path.Combine(RuntimeBaseDirectory, UserDataDirectoryName);

    public static string TempDirectory => Path.Combine(RuntimeBaseDirectory, TempDirectoryName);

    public static string SpellImageResourcesDirectory => Path.Combine(RuntimeBaseDirectory, SpellImageResourcesDirectoryName);

    public static string LogsDirectory => Path.Combine(RuntimeBaseDirectory, LogsDirectoryName);

    public static string ImageResourcesDirectory => Path.Combine(RuntimeBaseDirectory, ImageResourcesDirectoryName);

    public static string GameFilesDirectory => Path.Combine(RuntimeBaseDirectory, GameFilesDirectoryName);

    public static string SettingsFile => Path.Combine(RuntimeBaseDirectory, SettingsFileName);

    public static string LogFilePattern => Path.Combine(LogsDirectory, LogFilePatternName);

    public static string SoundDirectory => InstallationFile(SoundDirectoryName);

    public static bool IsDebugBuild
    {
        get
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }

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

    internal static IDisposable UseRuntimeBaseDirectoryForTests(string runtimeBaseDirectory)
    {
        var previousRuntimeBaseDirectoryOverride = _runtimeBaseDirectoryOverride;
        _runtimeBaseDirectoryOverride = runtimeBaseDirectory;
        return new RuntimeBaseDirectoryOverrideScope(previousRuntimeBaseDirectoryOverride);
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
}
