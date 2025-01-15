using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Backup;

public static class BackupController
{
    private static bool _isBackupRunning;
    
    public static bool Save()
    {
        if (_isBackupRunning)
        {
            return false;
        }

        _isBackupRunning = true;

        if (!CreateBackupDirWhenNotExist())
        {
            return false;
        }

        var sourceFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName);
        var backupDirPath = SettingsController.CurrentSettings.BackupStorageDirectoryPath;
        var backupFilePath = Path.Combine(backupDirPath, GetBackupFileName());

        try
        {
            string[] filesToZip = Directory.GetFiles(sourceFolderPath, "*.json", SearchOption.AllDirectories);

            using var zipArchive = ZipFile.Open(backupFilePath, ZipArchiveMode.Create);
            foreach (string file in filesToZip)
            {
                var entryName = file.Replace(sourceFolderPath, "").TrimStart(Path.DirectorySeparatorChar);
                zipArchive.CreateEntryFromFile(file, entryName);
            }

            ConsoleManager.WriteLineForMessage(LocalizationController.Translation("BACKUP_CREATED"));
            _ = ServiceLocator.Resolve<SatNotificationManager>()
                .ShowTrackingStatusAsync(LocalizationController.Translation("BACKUP_CREATED"), LocalizationController.Translation("A_BACKUP_HAS_BEEN_CREATED"));
            _isBackupRunning = false;
            return true;
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            _isBackupRunning = false;
            return false;
        }
    }

    private static string GetBackupFileName()
    {
        var date = DateTime.UtcNow;
        var secondsOfDay = (int) Math.Round((date - date.Date).TotalMilliseconds);
        return $"{date:yyyy}{date:MM}{date:dd}-{secondsOfDay}-UserData-backup.zip";
    }

    public static bool ExistBackupOnSettingConditions()
    {
        var backupDirPath = SettingsController.CurrentSettings.BackupStorageDirectoryPath;

        if (!Directory.Exists(backupDirPath))
        {
            return false;
        }

        var backupFiles = Directory.GetFiles(backupDirPath, "*.zip")
            .Select(filePath => new FileInfo(filePath))
            .OrderByDescending(fileInfo => fileInfo.LastWriteTimeUtc)
            .ToList();

        if (backupFiles.Count == 0)
        {
            return false;
        }

        var newestBackup = backupFiles.FirstOrDefault();

        if (newestBackup == null)
        {
            return false;
        }

        var currentDate = DateTime.UtcNow;

        var sevenDaysAgo = currentDate.AddDays(SettingsController.CurrentSettings.BackupIntervalByDays);
        return newestBackup.LastWriteTimeUtc >= sevenDaysAgo;
    }

    public static async Task DeleteOldestBackupsIfNeededAsync()
    {
        var backupDirPath = SettingsController.CurrentSettings.BackupStorageDirectoryPath;

        if (!Directory.Exists(backupDirPath))
        {
            return;
        }

        var maxBackups = SettingsController.CurrentSettings.MaximumNumberOfBackups;
        if (maxBackups <= 0)
        {
            return;
        }

        try
        {
            var backupFiles = Directory.GetFiles(backupDirPath, "*.zip")
                .Select(filePath => new FileInfo(filePath))
                .OrderBy(fileInfo => fileInfo.LastWriteTimeUtc)
                .ToList();

            if (backupFiles.Count <= maxBackups)
            {
                return;
            }

            int backupsToDeleteCount = backupFiles.Count - maxBackups;
            for (int i = 0; i < backupsToDeleteCount; i++)
            {
                await DeleteBackupAsync(backupFiles[i].FullName);
            }
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static async Task DeleteBackupAsync(string filePath)
    {
        try
        {
            await Task.Run(() => File.Delete(filePath));
        }
        catch (IOException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static bool CreateBackupDirWhenNotExist()
    {
        try
        {
            string defaultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.BackupDirectoryName);
            var backupDirPath = SettingsController.CurrentSettings.BackupStorageDirectoryPath;

            if (string.IsNullOrEmpty(backupDirPath))
            {
                backupDirPath = defaultPath;
                SettingsController.CurrentSettings.BackupStorageDirectoryPath = defaultPath;
            }
            
            return DirectoryController.CreateDirectoryWhenNotExists(backupDirPath);
        }
        catch (IOException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return false;
        }
    }
}