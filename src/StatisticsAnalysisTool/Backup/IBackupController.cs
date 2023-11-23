using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Backup;

public interface IBackupController
{
    bool SaveWithConditions();
    bool Save();
    bool ExistBackupOnSettingConditions();
    Task DeleteOldestBackupsIfNeededAsync();
}