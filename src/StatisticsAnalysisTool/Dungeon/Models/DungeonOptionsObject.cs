using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class DungeonOptionsObject : BaseViewModel
{
    private bool _isDungeonPlayerLootVisible;

    public event EventHandler PlayerLootVisibilityChanged;

    public DungeonOptionsObject()
    {
        IsDungeonPlayerLootVisible = SettingsController.CurrentSettings.IsDungeonPlayerLootVisible;
    }

    public bool IsDungeonPlayerLootVisible
    {
        get => _isDungeonPlayerLootVisible;
        set
        {
            if (_isDungeonPlayerLootVisible == value)
            {
                return;
            }

            _isDungeonPlayerLootVisible = value;
            SettingsController.CurrentSettings.IsDungeonPlayerLootVisible = _isDungeonPlayerLootVisible;
            OnPropertyChanged();
            PlayerLootVisibilityChanged?.Invoke(this, EventArgs.Empty);
        }
    }
    public void RefreshLocalization()
    {
        OnPropertyChanged(null);
    }

    public static string TranslationSettings => LocalizationController.Translation("SETTINGS");
    public static string TranslationDungeonPlayerLootVisible => LocalizationController.Translation("DUNGEON_PLAYER_LOOT_VISIBLE");
}
