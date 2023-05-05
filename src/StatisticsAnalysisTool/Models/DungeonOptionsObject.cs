using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models;

public class DungeonOptionsObject : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}