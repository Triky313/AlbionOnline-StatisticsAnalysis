using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models;

public class DungeonStatsFilter : BaseViewModel
{
    private bool? _soloCheckbox = true;
    private bool? _standardCheckbox = true;
    private bool? _avaCheckbox = true;
    private bool? _hgCheckbox = true;
    private bool? _expeditionCheckbox = true;
    private bool? _corruptedCheckbox = true;
    private bool? _unknownCheckbox = true;
    private List<DungeonMode> _dungeonModeFilters = new()
        {
            DungeonMode.Solo,
            DungeonMode.Standard,
            DungeonMode.Avalon,
            DungeonMode.HellGate,
            DungeonMode.Expedition,
            DungeonMode.Corrupted,
            DungeonMode.Mists,
            DungeonMode.MistsDungeon,
            DungeonMode.Unknown
        };
    private bool? _isTierUnknown = true;
    private bool? _isT1 = true;
    private bool? _isT2 = true;
    private bool? _isT3 = true;
    private bool? _isT4 = true;
    private bool? _isT5 = true;
    private bool? _isT6 = true;
    private bool? _isT7 = true;
    private bool? _isT8 = true;
    private List<Tier> _tierFilters = new()
        {
            Tier.Unknown,
            Tier.T1,
            Tier.T2,
            Tier.T3,
            Tier.T4,
            Tier.T5,
            Tier.T6,
            Tier.T7,
            Tier.T8
        };
    private bool? _isLevelUnknown = true;
    private bool? _isLevel0 = true;
    private bool? _isLevel1 = true;
    private bool? _isLevel2 = true;
    private bool? _isLevel3 = true;
    private bool? _isLevel4 = true;
    private List<ItemLevel> _levelFilters = new()
        {
            ItemLevel.Unknown,
            ItemLevel.Level0,
            ItemLevel.Level1,
            ItemLevel.Level2,
            ItemLevel.Level3,
            ItemLevel.Level4
        };

    private readonly TrackingController _trackingController;

    public DungeonStatsFilter(TrackingController trackingController)
    {
        _trackingController = trackingController;

        _trackingController?.DungeonController?.UpdateDungeonStatsUi();
        _trackingController?.DungeonController?.UpdateDungeonChestsUi();
    }

    #region Dungeon

    public bool? SoloCheckbox
    {
        get => _soloCheckbox;
        set
        {
            _soloCheckbox = value;
            ChangeDungeonModeFilter(DungeonMode.Solo, _soloCheckbox ?? false);
            OnPropertyChanged();
        }
    }

    public bool? StandardCheckbox
    {
        get => _standardCheckbox;
        set
        {
            _standardCheckbox = value;
            ChangeDungeonModeFilter(DungeonMode.Standard, _standardCheckbox ?? false);
            OnPropertyChanged();
        }
    }

    public bool? AvaCheckbox
    {
        get => _avaCheckbox;
        set
        {
            _avaCheckbox = value;
            ChangeDungeonModeFilter(DungeonMode.Avalon, _avaCheckbox ?? false);
            OnPropertyChanged();
        }
    }

    public bool? HgCheckbox
    {
        get => _hgCheckbox;
        set
        {
            _hgCheckbox = value;
            ChangeDungeonModeFilter(DungeonMode.HellGate, _hgCheckbox ?? false);
            OnPropertyChanged();
        }
    }

    public bool? ExpeditionCheckbox
    {
        get => _expeditionCheckbox;
        set
        {
            _expeditionCheckbox = value;
            ChangeDungeonModeFilter(DungeonMode.Expedition, _expeditionCheckbox ?? false);
            OnPropertyChanged();
        }
    }

    public bool? CorruptedCheckbox
    {
        get => _corruptedCheckbox;
        set
        {
            _corruptedCheckbox = value;
            ChangeDungeonModeFilter(DungeonMode.Corrupted, _corruptedCheckbox ?? false);
            OnPropertyChanged();
        }
    }

    public bool? UnknownCheckbox
    {
        get => _unknownCheckbox;
        set
        {
            _unknownCheckbox = value;
            ChangeDungeonModeFilter(DungeonMode.Unknown, _unknownCheckbox ?? false);
            OnPropertyChanged();
        }
    }

    public List<DungeonMode> DungeonModeFilters
    {
        get => _dungeonModeFilters;
        set
        {
            _dungeonModeFilters = value;
            OnPropertyChanged();
        }
    }

    private void ChangeDungeonModeFilter(DungeonMode dungeonMode, bool filterStatus)
    {
        if (filterStatus)
        {
            AddDungeonMode(dungeonMode);
        }
        else
        {
            RemoveDungeonMode(dungeonMode);
        }
    }

    private void AddDungeonMode(DungeonMode dungeonMode)
    {
        if (!_dungeonModeFilters.Exists(x => x == dungeonMode))
        {
            _dungeonModeFilters.Add(dungeonMode);
        }
    }

    private void RemoveDungeonMode(DungeonMode dungeonMode)
    {
        if (_dungeonModeFilters.Exists(x => x == dungeonMode))
        {
            _dungeonModeFilters.Remove(dungeonMode);
        }
    }

    #endregion

    #region Tier

    public bool? IsTierUnknown
    {
        get => _isTierUnknown;
        set
        {
            _isTierUnknown = value;
            ChangeTierFilter(Tier.Unknown, _isTierUnknown ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT1
    {
        get => _isT1;
        set
        {
            _isT1 = value;
            ChangeTierFilter(Tier.T1, _isT1 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT2
    {
        get => _isT2;
        set
        {
            _isT2 = value;
            ChangeTierFilter(Tier.T2, _isT2 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT3
    {
        get => _isT3;
        set
        {
            _isT3 = value;
            ChangeTierFilter(Tier.T3, _isT3 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT4
    {
        get => _isT4;
        set
        {
            _isT4 = value;
            ChangeTierFilter(Tier.T4, _isT4 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT5
    {
        get => _isT5;
        set
        {
            _isT5 = value;
            ChangeTierFilter(Tier.T5, _isT5 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT6
    {
        get => _isT6;
        set
        {
            _isT6 = value;
            ChangeTierFilter(Tier.T6, _isT6 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT7
    {
        get => _isT7;
        set
        {
            _isT7 = value;
            ChangeTierFilter(Tier.T7, _isT7 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsT8
    {
        get => _isT8;
        set
        {
            _isT8 = value;
            ChangeTierFilter(Tier.T8, _isT8 ?? false);
            OnPropertyChanged();
        }
    }

    public List<Tier> TierFilters
    {
        get => _tierFilters;
        set
        {
            _tierFilters = value;
            OnPropertyChanged();
        }
    }

    private void ChangeTierFilter(Tier tier, bool filterStatus)
    {
        if (filterStatus)
        {
            AddTier(tier);
        }
        else
        {
            RemoveTier(tier);
        }
    }

    private void AddTier(Tier tier)
    {
        if (!_tierFilters.Exists(x => x == tier))
        {
            _tierFilters.Add(tier);
        }
    }

    private void RemoveTier(Tier tier)
    {
        if (_tierFilters.Exists(x => x == tier))
        {
            _tierFilters.Remove(tier);
        }
    }

    #endregion

    #region Level

    public bool? IsLevelUnknown
    {
        get => _isLevelUnknown;
        set
        {
            _isLevelUnknown = value;
            ChangeTierFilter(ItemLevel.Unknown, _isLevelUnknown ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsLevel0
    {
        get => _isLevel0;
        set
        {
            _isLevel0 = value;
            ChangeTierFilter(ItemLevel.Level0, _isLevel0 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsLevel1
    {
        get => _isLevel1;
        set
        {
            _isLevel1 = value;
            ChangeTierFilter(ItemLevel.Level1, _isLevel1 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsLevel2
    {
        get => _isLevel2;
        set
        {
            _isLevel2 = value;
            ChangeTierFilter(ItemLevel.Level2, _isLevel2 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsLevel3
    {
        get => _isLevel3;
        set
        {
            _isLevel3 = value;
            ChangeTierFilter(ItemLevel.Level3, _isLevel3 ?? false);
            OnPropertyChanged();
        }
    }

    public bool? IsLevel4
    {
        get => _isLevel4;
        set
        {
            _isLevel4 = value;
            ChangeTierFilter(ItemLevel.Level4, _isLevel4 ?? false);
            OnPropertyChanged();
        }
    }

    public List<ItemLevel> LevelFilters
    {
        get => _levelFilters;
        set
        {
            _levelFilters = value;
            OnPropertyChanged();
        }
    }

    private void ChangeTierFilter(ItemLevel level, bool filterStatus)
    {
        if (filterStatus)
        {
            AddTier(level);
        }
        else
        {
            RemoveTier(level);
        }
    }

    private void AddTier(ItemLevel level)
    {
        if (!_levelFilters.Exists(x => x == level))
        {
            _levelFilters.Add(level);
        }
    }

    private void RemoveTier(ItemLevel level)
    {
        if (_levelFilters.Exists(x => x == level))
        {
            _levelFilters.Remove(level);
        }
    }

    #endregion

    public static string TranslationFilter => LanguageController.Translation("FILTER");
    public static string TranslationSolo => LanguageController.Translation("SOLO");
    public static string TranslationSoloDungeon => LanguageController.Translation("SOLO_DUNGEON");
    public static string TranslationStandard => LanguageController.Translation("STANDARD");
    public static string TranslationStandardDungeon => LanguageController.Translation("STANDARD_DUNGEON");
    public static string TranslationAva => LanguageController.Translation("AVA");
    public static string TranslationAvalonianDungeon => LanguageController.Translation("AVALONIAN_DUNGEON");
    public static string TranslationHg => LanguageController.Translation("HG");
    public static string TranslationHellGate => LanguageController.Translation("HELLGATE");
    public static string TranslationCorrupted => LanguageController.Translation("CORRUPTED");
    public static string TranslationCorruptedDungeon => LanguageController.Translation("CORRUPTED_LAIR");
    public static string TranslationExped => LanguageController.Translation("EXPED");
    public static string TranslationExpedition => LanguageController.Translation("EXPEDITION");
    public static string TranslationUnknown => LanguageController.Translation("UNKNOWN");
    public static string TranslationT1 => LanguageController.Translation("T1");
    public static string TranslationT2 => LanguageController.Translation("T2");
    public static string TranslationT3 => LanguageController.Translation("T3");
    public static string TranslationT4 => LanguageController.Translation("T4");
    public static string TranslationT5 => LanguageController.Translation("T5");
    public static string TranslationT6 => LanguageController.Translation("T6");
    public static string TranslationT7 => LanguageController.Translation("T7");
    public static string TranslationT8 => LanguageController.Translation("T8");
    public static string TranslationFlat => LanguageController.Translation("Flat");
    public static string TranslationLevelPoint0 => LanguageController.Translation("LevelPoint0");
    public static string TranslationLevelPoint1 => LanguageController.Translation("LevelPoint1");
    public static string TranslationLevelPoint2 => LanguageController.Translation("LevelPoint2");
    public static string TranslationLevelPoint3 => LanguageController.Translation("LevelPoint3");
}