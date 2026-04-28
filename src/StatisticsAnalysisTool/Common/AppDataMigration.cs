using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace StatisticsAnalysisTool.Common;

public static class AppDataMigration
{
    public static IReadOnlyCollection<AppDataMigrationMessage> MigrateLegacyRuntimeData()
    {
        var messages = new List<AppDataMigrationMessage>();

        if (!TryGetMigrationSource(out var source))
        {
            return messages;
        }

        TryMigrateDirectory(source.BackupsDirectory, AppDataPaths.BackupsDirectory, messages);
        TryMigrateDirectory(source.UserDataDirectory, AppDataPaths.UserDataDirectory, messages);
        TryMigrateDirectory(source.TempDirectory, AppDataPaths.TempDirectory, messages);
        TryMigrateDirectory(source.SpellImageResourcesDirectory, AppDataPaths.SpellImageResourcesDirectory, messages);
        TryMigrateDirectory(source.LogsDirectory, AppDataPaths.LogsDirectory, messages);
        TryMigrateDirectory(source.ImageResourcesDirectory, AppDataPaths.ImageResourcesDirectory, messages);
        TryMigrateDirectory(source.GameFilesDirectory, AppDataPaths.GameFilesDirectory, messages);
        TryMigrateFile(source.SettingsFile, AppDataPaths.SettingsFile, messages);

        return messages;
    }

    public static void LogMessages(IEnumerable<AppDataMigrationMessage> messages)
    {
        foreach (var message in messages)
        {
            if (message.IsError)
            {
                Log.Warning(message.Exception, "Migration from {source} to {target} failed.", message.SourcePath, message.TargetPath);
                continue;
            }

            Log.Information("Migrated runtime data from {source} to {target}.", message.SourcePath, message.TargetPath);
        }
    }

    private static bool TryGetMigrationSource(out AppDataMigrationSource source)
    {
        var installationSource = AppDataMigrationSource.FromInstallationDirectory();
        if (HasRuntimeData(installationSource))
        {
            source = installationSource;
            return true;
        }

        var legacyDefaultSource = AppDataMigrationSource.FromLegacyDefaultDirectory();
        if (HasRuntimeData(legacyDefaultSource))
        {
            source = legacyDefaultSource;
            return true;
        }

        source = AppDataMigrationSource.Empty;
        return false;
    }

    private static bool HasRuntimeData(AppDataMigrationSource source)
    {
        return Directory.Exists(source.BackupsDirectory)
            || Directory.Exists(source.UserDataDirectory)
            || Directory.Exists(source.TempDirectory)
            || Directory.Exists(source.SpellImageResourcesDirectory)
            || Directory.Exists(source.LogsDirectory)
            || Directory.Exists(source.ImageResourcesDirectory)
            || Directory.Exists(source.GameFilesDirectory)
            || File.Exists(source.SettingsFile);
    }

    private static void TryMigrateDirectory(string sourcePath, string targetPath, ICollection<AppDataMigrationMessage> messages)
    {
        try
        {
            if (!Directory.Exists(sourcePath) || Directory.Exists(targetPath))
            {
                return;
            }

            CopyDirectory(sourcePath, targetPath);
            messages.Add(AppDataMigrationMessage.Success(sourcePath, targetPath));
        }
        catch (Exception ex)
        {
            messages.Add(AppDataMigrationMessage.Error(sourcePath, targetPath, ex));
        }
    }

    private static void TryMigrateFile(string sourcePath, string targetPath, ICollection<AppDataMigrationMessage> messages)
    {
        try
        {
            if (!File.Exists(sourcePath) || File.Exists(targetPath))
            {
                return;
            }

            var targetDirectory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrWhiteSpace(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            File.Copy(sourcePath, targetPath, overwrite: false);
            messages.Add(AppDataMigrationMessage.Success(sourcePath, targetPath));
        }
        catch (Exception ex)
        {
            messages.Add(AppDataMigrationMessage.Error(sourcePath, targetPath, ex));
        }
    }

    private static void CopyDirectory(string sourcePath, string targetPath)
    {
        Directory.CreateDirectory(targetPath);

        foreach (var sourceFilePath in Directory.GetFiles(sourcePath))
        {
            var targetFilePath = Path.Combine(targetPath, Path.GetFileName(sourceFilePath));
            File.Copy(sourceFilePath, targetFilePath, overwrite: false);
        }

        foreach (var sourceDirectoryPath in Directory.GetDirectories(sourcePath))
        {
            var targetDirectoryPath = Path.Combine(targetPath, Path.GetFileName(sourceDirectoryPath));
            CopyDirectory(sourceDirectoryPath, targetDirectoryPath);
        }
    }

    private sealed class AppDataMigrationSource
    {
        private AppDataMigrationSource(
            string backupsDirectory,
            string userDataDirectory,
            string tempDirectory,
            string spellImageResourcesDirectory,
            string logsDirectory,
            string imageResourcesDirectory,
            string gameFilesDirectory,
            string settingsFile)
        {
            BackupsDirectory = backupsDirectory;
            UserDataDirectory = userDataDirectory;
            TempDirectory = tempDirectory;
            SpellImageResourcesDirectory = spellImageResourcesDirectory;
            LogsDirectory = logsDirectory;
            ImageResourcesDirectory = imageResourcesDirectory;
            GameFilesDirectory = gameFilesDirectory;
            SettingsFile = settingsFile;
        }

        public string BackupsDirectory { get; }

        public string UserDataDirectory { get; }

        public string TempDirectory { get; }

        public string SpellImageResourcesDirectory { get; }

        public string LogsDirectory { get; }

        public string ImageResourcesDirectory { get; }

        public string GameFilesDirectory { get; }

        public string SettingsFile { get; }

        public static AppDataMigrationSource Empty { get; } = new(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        public static AppDataMigrationSource FromInstallationDirectory()
        {
            return new AppDataMigrationSource(
                AppDataPaths.LegacyBackupsDirectory,
                AppDataPaths.LegacyUserDataDirectory,
                AppDataPaths.LegacyTempDirectory,
                AppDataPaths.LegacySpellImageResourcesDirectory,
                AppDataPaths.LegacyLogsDirectory,
                AppDataPaths.LegacyImageResourcesDirectory,
                AppDataPaths.LegacyGameFilesDirectory,
                AppDataPaths.LegacySettingsFile);
        }

        public static AppDataMigrationSource FromLegacyDefaultDirectory()
        {
            return new AppDataMigrationSource(
                AppDataPaths.LegacyDefaultBackupsDirectory,
                AppDataPaths.LegacyDefaultUserDataDirectory,
                AppDataPaths.LegacyDefaultTempDirectory,
                AppDataPaths.LegacyDefaultSpellImageResourcesDirectory,
                AppDataPaths.LegacyDefaultLogsDirectory,
                AppDataPaths.LegacyDefaultImageResourcesDirectory,
                AppDataPaths.LegacyDefaultGameFilesDirectory,
                AppDataPaths.LegacyDefaultSettingsFile);
        }
    }
}

public sealed class AppDataMigrationMessage
{
    private AppDataMigrationMessage(string sourcePath, string targetPath, Exception exception)
    {
        SourcePath = sourcePath;
        TargetPath = targetPath;
        Exception = exception;
    }

    public string SourcePath { get; }

    public string TargetPath { get; }

    public Exception Exception { get; }

    public bool IsError => Exception is not null;

    public static AppDataMigrationMessage Success(string sourcePath, string targetPath)
    {
        return new AppDataMigrationMessage(sourcePath, targetPath, null);
    }

    public static AppDataMigrationMessage Error(string sourcePath, string targetPath, Exception exception)
    {
        return new AppDataMigrationMessage(sourcePath, targetPath, exception);
    }
}