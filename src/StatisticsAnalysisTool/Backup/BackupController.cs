using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Backup;

public static class BackupController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private static bool _isBackupRunning;

    public static bool Save()
    {
        return Task.Run(SaveAsync).GetAwaiter().GetResult();
    }

    public static async Task<bool> SaveAsync()
    {
        if (_isBackupRunning)
        {
            return false;
        }

        return await Task.Run(async () =>
        {
            _isBackupRunning = true;
            var sourceFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName);
            var backupDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.BackupDirectoryName);

            if (!DirectoryController.CreateDirectoryWhenNotExists(backupDirPath))
            {
                return false;
            }

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

                ConsoleManager.WriteLineForMessage(LanguageController.Translation("BACKUP_CREATED"));
                await ServiceLocator.Resolve<SatNotificationManager>()
                    .ShowTrackingStatusAsync(LanguageController.Translation("BACKUP_CREATED"), LanguageController.Translation("A_BACKUP_HAS_BEEN_CREATED"));
                _isBackupRunning = false;
                return true;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                _isBackupRunning = false;
                return false;
            }
        });
    }

    private static string GetBackupFileName()
    {
        var date = DateTime.UtcNow;
        var secondsOfDay = (int) Math.Round((date - date.Date).TotalSeconds);
        return $"{date:yyyy}{date:MM}{date:dd}-{secondsOfDay}-UserData-backup.zip";
    }
}