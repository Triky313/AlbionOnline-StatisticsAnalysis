using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using StatisticsAnalysisTool.Enumerations;

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
        TryMigrateDirectory(source.TempDirectory, AppDataPaths.TempDirectory, messages);
        TryMigrateDirectory(source.SpellImageResourcesDirectory, AppDataPaths.SpellImageResourcesDirectory, messages);
        TryMigrateDirectory(source.LogsDirectory, AppDataPaths.LogsDirectory, messages);
        TryMigrateDirectory(source.ImageResourcesDirectory, AppDataPaths.ImageResourcesDirectory, messages);
        TryMigrateDirectory(source.GameFilesDirectory, AppDataPaths.GameFilesDirectory, messages);
        TryMigrateFile(source.SettingsFile, AppDataPaths.SettingsFile, messages);

        return messages;
    }

    public static bool TryMigrateLegacyUserDataToServerDirectory(
        ServerLocation serverLocation,
        out bool sourceExists,
        out IReadOnlyCollection<AppDataMigrationMessage> messages)
    {
        var migrationMessages = new List<AppDataMigrationMessage>();
        messages = migrationMessages;
        sourceExists = false;

        if (serverLocation is not (ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe))
        {
            return false;
        }

        if (!TryGetLegacyUserDataMigrationSource(out var sourcePath))
        {
            return false;
        }

        sourceExists = true;

        var targetPath = AppDataPaths.GetUserDataDirectory(serverLocation);
        var migrationId = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var tempTargetPath = $"{targetPath}.migration-{migrationId}";
        var backupTargetPath = $"{targetPath}.before-migration-{migrationId}";

        try
        {
            Log.Information("Migrating legacy user data. Server={Server}, Source={Source}, Target={Target}", serverLocation, sourcePath, targetPath);

            CopyDirectory(sourcePath, tempTargetPath, true);

            if (Directory.Exists(targetPath))
            {
                Directory.Move(targetPath, backupTargetPath);
                Log.Information("Existing server user data directory moved before legacy migration. Source={Source}, Target={Target}", targetPath, backupTargetPath);
            }

            Directory.Move(tempTargetPath, targetPath);
            migrationMessages.Add(AppDataMigrationMessage.Success(sourcePath, targetPath));
            return true;
        }
        catch (Exception ex)
        {
            SafeDeleteDirectory(tempTargetPath);
            TryRestoreBackupDirectory(backupTargetPath, targetPath);
            migrationMessages.Add(AppDataMigrationMessage.Error(sourcePath, targetPath, ex));
            return false;
        }
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

    private static bool TryGetLegacyUserDataMigrationSource(out string sourcePath)
    {
        foreach (var candidatePath in new[]
                 {
                     AppDataPaths.LegacyRuntimeUserDataDirectory,
                     AppDataPaths.LegacyUserDataDirectory,
                     AppDataPaths.LegacyDefaultUserDataDirectory
                 })
        {
            if (ContainsLegacyUserData(candidatePath))
            {
                sourcePath = candidatePath;
                return true;
            }
        }

        sourcePath = string.Empty;
        return false;
    }

    private static bool HasRuntimeData(AppDataMigrationSource source)
    {
        return Directory.Exists(source.BackupsDirectory)
            || ContainsLegacyUserData(source.UserDataDirectory)
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

            CopyDirectory(sourcePath, targetPath, false);
            messages.Add(AppDataMigrationMessage.Success(sourcePath, targetPath));
        }
        catch (Exception ex)
        {
            messages.Add(AppDataMigrationMessage.Error(sourcePath, targetPath, ex));
        }
    }

    private static bool ContainsLegacyUserData(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        try
        {
            return !IsDirectoryEmpty(path);
        }
        catch
        {
            return true;
        }
    }

    private static bool IsDirectoryEmpty(string path)
    {
        using var entries = Directory.EnumerateFileSystemEntries(path).GetEnumerator();
        return !entries.MoveNext();
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

    private static void CopyDirectory(string sourcePath, string targetPath, bool overwrite)
    {
        Directory.CreateDirectory(targetPath);

        foreach (var sourceFilePath in Directory.GetFiles(sourcePath))
        {
            var targetFilePath = Path.Combine(targetPath, Path.GetFileName(sourceFilePath));
            File.Copy(sourceFilePath, targetFilePath, overwrite);
        }

        foreach (var sourceDirectoryPath in Directory.GetDirectories(sourcePath))
        {
            var targetDirectoryPath = Path.Combine(targetPath, Path.GetFileName(sourceDirectoryPath));
            CopyDirectory(sourceDirectoryPath, targetDirectoryPath, overwrite);
        }
    }

    private static void SafeDeleteDirectory(string path)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {
            // ignored
        }
    }

    private static void TryRestoreBackupDirectory(string backupPath, string targetPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(backupPath) || !Directory.Exists(backupPath) || Directory.Exists(targetPath))
            {
                return;
            }

            Directory.Move(backupPath, targetPath);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Could not restore server user data backup after failed migration. Backup={Backup}, Target={Target}", backupPath, targetPath);
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
