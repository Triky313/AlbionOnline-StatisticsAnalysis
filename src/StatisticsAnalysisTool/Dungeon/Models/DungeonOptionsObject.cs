using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class DungeonOptionsObject : BaseViewModel
{
    private bool _isDungeonClosedSoundActive;

    public DungeonOptionsObject()
    {
        IsDungeonClosedSoundActive = SettingsController.CurrentSettings.IsDungeonClosedSoundActive;
    }

    public bool IsDungeonClosedSoundActive
    {
        get => _isDungeonClosedSoundActive;
        set
        {
            _isDungeonClosedSoundActive = value;
            SettingsController.CurrentSettings.IsDungeonClosedSoundActive = _isDungeonClosedSoundActive;
            OnPropertyChanged();
        }
    }

    public static string TranslationSettings => LanguageController.Translation("SETTINGS");
    public static string TranslationDungeonClosedSoundActive => LanguageController.Translation("DUNGEON_CLOSED_SOUND_ACTIVE");
}