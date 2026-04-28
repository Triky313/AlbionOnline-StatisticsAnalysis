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

        TryMigrateDirectory(AppDataPaths.LegacyBackupsDirectory, AppDataPaths.BackupsDirectory, messages);
        TryMigrateDirectory(AppDataPaths.LegacyUserDataDirectory, AppDataPaths.UserDataDirectory, messages);
        TryMigrateDirectory(AppDataPaths.LegacyTempDirectory, AppDataPaths.TempDirectory, messages);
        TryMigrateDirectory(AppDataPaths.LegacySpellImageResourcesDirectory, AppDataPaths.SpellImageResourcesDirectory, messages);
        TryMigrateDirectory(AppDataPaths.LegacyLogsDirectory, AppDataPaths.LogsDirectory, messages);
        TryMigrateDirectory(AppDataPaths.LegacyImageResourcesDirectory, AppDataPaths.ImageResourcesDirectory, messages);
        TryMigrateDirectory(AppDataPaths.LegacyGameFilesDirectory, AppDataPaths.GameFilesDirectory, messages);
        TryMigrateFile(AppDataPaths.LegacySettingsFile, AppDataPaths.SettingsFile, messages);

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