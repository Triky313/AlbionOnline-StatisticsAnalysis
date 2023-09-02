using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class PointOfInterest : BaseViewModel
{
    public int Id { get; set; }
    public DateTime Opened { get; set; }
    private bool _isBossChest;
    private TreasureRarity _rarity;
    private ChestStatus _status;
    private EventType _type;
    private string _uniqueName;
    private ShrineType _shrineType;
    private ShrineBuff _shrineBuff;
    public string Hash => $"{Id}{UniqueName}";

    public PointOfInterest(int id, string uniqueName)
    {
        Id = id;
        UniqueName = uniqueName;
        Rarity = DungeonData.GetChestRarity(UniqueName);
        ShrineBuff = DungeonData.GetShrineBuff(UniqueName);
        ShrineType = DungeonData.GetShrineType(UniqueName);
        Type = DungeonData.GetDungeonEventType(UniqueName);
        IsBossChest = DungeonData.IsBossChest(UniqueName);
        Status = ChestStatus.Close;
    }

    public PointOfInterest()
    {
    }

    public string UniqueName
    {
        get => _uniqueName;
        set
        {
            _uniqueName = value;
            OnPropertyChanged();
        }
    }

    public EventType Type
    {
        get => _type;
        set
        {
            _type = value;
            OnPropertyChanged();
        }
    }

    public TreasureRarity Rarity
    {
        get => _rarity;
        set
        {
            _rarity = value;
            OnPropertyChanged();
        }
    }

    public ChestStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public ShrineType ShrineType
    {
        get => _shrineType;
        set
        {
            _shrineType = value;
            OnPropertyChanged();
        }
    }

    public ShrineBuff ShrineBuff
    {
        get => _shrineBuff;
        set
        {
            _shrineBuff = value;
            OnPropertyChanged();
        }
    }

    public bool IsBossChest
    {
        get => _isBossChest;
        set
        {
            _isBossChest = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationStandard => LanguageController.Translation("STANDARD");
    public static string TranslationUncommon => LanguageController.Translation("UNCOMMON");
    public static string TranslationRare => LanguageController.Translation("RARE");
    public static string TranslationLegendary => LanguageController.Translation("LEGENDARY");
    public static string TranslationBossChest => LanguageController.Translation("BOSS_CHEST");
    public static string TranslationBookChest => LanguageController.Translation("BOOK_CHEST");
    public static string TranslationCombatBuff => LanguageController.Translation("COMBAT_BUFF");
    public static string TranslationSilverShrine => LanguageController.Translation("SILVER_SHRINE");
    public static string TranslationFameShrine => LanguageController.Translation("FAME_SHRINE");
    public static string TranslationLockedChest => LanguageController.Translation("LOCKED_CHEST");
}