using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace StatisticsAnalysisTool.Dungeon.Models;

public abstract class DungeonBaseFragment : BaseViewModel
{
    private int _count;
    private Faction _faction = Faction.Unknown;
    private DungeonStatus _status;
    private Visibility _visibility = Visibility.Visible;
    private DateTime _enterDungeonFirstTime;
    private DungeonMode _mode = DungeonMode.Unknown;
    private Tier _tier = Tier.Unknown;
    private int _level = -1;
    private string _mainMapName;
    private bool? _isSelectedForDeletion = false;
    private double _fame;
    private double _reSpec;
    private double _silver;
    private List<TimeCollectObject> _dungeonRunTimes = new();
    private int _totalRunTimeInSeconds;
    private ObservableCollection<DungeonEvent> _events = new ();
    private ObservableCollection<Loot> _loot = new();
    private string _diedName;
    private string _killedBy;
    private MapType _mapType = MapType.Unknown;
    private double _totalValue;
    private double _famePerHour;
    private double _reSpecPerHour;
    private double _silverPerHour;
    private string _mainMapIndex;

    public ObservableCollection<Guid> GuidList { get; set; }
    public string DungeonHash => $"{EnterDungeonFirstTime.Ticks}{string.Join(",", GuidList)}";

    protected DungeonBaseFragment(Guid guid, MapType mapType, DungeonMode mode, string mainMapIndex)
    {
        GuidList = new ObservableCollection<Guid>() { guid };
        MapType = mapType;
        Mode = mode;
        MainMapIndex = mainMapIndex;
        EnterDungeonFirstTime = DateTime.UtcNow;
        Status = DungeonStatus.Active;
        Visibility = Visibility.Visible;
    }

    protected DungeonBaseFragment(DungeonDto dto)
    {
        GuidList = new ObservableCollection<Guid>(dto.GuidList);
        AddTimer(new DateTime());
        MapType = dto.MapType;
        Mode = dto.Mode;
        Faction = dto.Faction;
        Status = DungeonStatus.Done;
        Visibility = Visibility.Visible;
        EnterDungeonFirstTime = dto.EnterDungeonFirstTime;
        Tier = dto.Tier;
        Level = dto.Level;
        Fame = dto.Fame;
        Silver = dto.Silver;
        ReSpec = dto.ReSpec;
        TotalRunTimeInSeconds = dto.TotalRunTimeInSeconds;
        Events = new ObservableCollection<DungeonEvent>(dto.Events.Select(DungeonMapping.Mapping));
        Loot = new ObservableCollection<Loot>(dto.Loot.Select(DungeonMapping.Mapping));

        Loot.CollectionChanged += UpdateLoot;
    }

    public int Count
    {
        get => _count;
        set
        {
            _count = value;
            OnPropertyChanged();
        }
    }

    public MapType MapType
    {
        get => _mapType;
        set
        {
            _mapType = value;
            SetModeAndFaction(_mapType);
            OnPropertyChanged();
        }
    }

    public Faction Faction
    {
        get => _faction;
        set
        {
            _faction = value;
            OnPropertyChanged();
        }
    }

    public DungeonStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
    }

    public DateTime EnterDungeonFirstTime
    {
        get => _enterDungeonFirstTime;
        set
        {
            _enterDungeonFirstTime = value;
            OnPropertyChanged();
        }
    }

    public DungeonMode Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            OnPropertyChanged();
        }
    }

    public Tier Tier
    {
        get => _tier;
        set
        {
            _tier = value;
            OnPropertyChanged();
        }
    }

    public int Level
    {
        get => _level;
        set
        {
            _level = value;
            OnPropertyChanged();
        }
    }

    public string MainMapIndex
    {
        get => _mainMapIndex;
        set
        {
            _mainMapIndex = value;
            MainMapName = WorldData.GetUniqueNameOrDefault(value);
            OnPropertyChanged();
        }
    }

    public bool? IsSelectedForDeletion
    {
        get => _isSelectedForDeletion;
        set
        {
            _isSelectedForDeletion = value;
            OnPropertyChanged();
        }
    }

    public double Fame
    {
        get => _fame;
        set
        {
            _fame = value;
            FamePerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            OnPropertyChanged();
        }
    }

    public double ReSpec
    {
        get => _reSpec;
        set
        {
            _reSpec = value;
            ReSpecPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            OnPropertyChanged();
        }
    }

    public double Silver
    {
        get => _silver;
        set
        {
            _silver = value;
            SilverPerHour = value.GetValuePerHour(TotalRunTimeInSeconds <= 0 ? (DateTime.UtcNow - EnterDungeonFirstTime).Seconds : TotalRunTimeInSeconds);
            UpdateLoot(null, null);
            OnPropertyChanged();
        }
    }

    public List<TimeCollectObject> DungeonRunTimes
    {
        get => _dungeonRunTimes;
        set
        {
            _dungeonRunTimes = value;
            OnPropertyChanged();
        }
    }

    public int TotalRunTimeInSeconds
    {
        get => _totalRunTimeInSeconds;
        set
        {
            _totalRunTimeInSeconds = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<DungeonEvent> Events
    {
        get => _events;
        set
        {
            _events = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Loot> Loot
    {
        get => _loot;
        set
        {
            _loot = value;
            OnPropertyChanged();
        }
    }

    public string DiedName
    {
        get => _diedName;
        set
        {
            _diedName = value;
            OnPropertyChanged();
        }
    }

    public string KilledBy
    {
        get => _killedBy;
        set
        {
            _killedBy = value;
            OnPropertyChanged();
        }
    }

    #region Composite values that are not in the DTO

    public string MainMapName
    {
        get => _mainMapName;
        set
        {
            _mainMapName = value;
            OnPropertyChanged();
        }
    }

    public double TotalValue
    {
        get => _totalValue;
        set
        {
            _totalValue = value;
            OnPropertyChanged();
        }
    }

    public double FamePerHour
    {
        get
        {
            if (double.IsNaN(_famePerHour))
            {
                return 0;
            }

            return _famePerHour;
        }
        private set
        {
            _famePerHour = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPerHour
    {
        get
        {
            if (double.IsNaN(_reSpecPerHour))
            {
                return 0;
            }

            return _reSpecPerHour;
        }
        private set
        {
            _reSpecPerHour = value;
            OnPropertyChanged();
        }
    }

    public double SilverPerHour
    {
        get
        {
            if (double.IsNaN(_silverPerHour))
            {
                return 0;
            }

            return _silverPerHour;
        }
        private set
        {
            _silverPerHour = value;
            OnPropertyChanged();
        }
    }

    #endregion

    private void UpdateLoot(object sender, NotifyCollectionChangedEventArgs e)
    {
        var lootValue = Loot?.Sum(x => FixPoint.FromInternalValue(x?.EstimatedMarketValueInternal ?? 0).DoubleValue) ?? 0;
        TotalValue = Silver + lootValue;
    }

    public static string SetTierString(Tier tier)
    {
        var tierString = tier switch
        {
            Tier.T1 => "T1",
            Tier.T2 => "T2",
            Tier.T3 => "T3",
            Tier.T4 => "T4",
            Tier.T5 => "T5",
            Tier.T6 => "T6",
            Tier.T7 => "T7",
            Tier.T8 => "T8",
            _ => "T?"
        };

        return tierString;
    }

    public static string SetLevelString(int level)
    {
        var levelString = level switch
        {
            0 => "",
            1 => ".0",
            2 => ".1",
            3 => ".2",
            4 => ".3",
            _ => ".?"
        };

        return levelString;
    }

    public void AddTimer(DateTime time)
    {
        if (DungeonRunTimes.Any(x => x.EndTime == null))
        {
            var dun = DungeonRunTimes.FirstOrDefault(x => x.EndTime == null);
            if (dun != null)
            {
                dun.EndTime = time;
                DungeonRunTimes.Add(new TimeCollectObject(time));
            }
        }
        else
        {
            DungeonRunTimes.Add(new TimeCollectObject(time));
        }

        SetTotalRunTimeInSeconds();
    }

    public void EndTimer()
    {
        var dateTime = DateTime.UtcNow;

        var dun = DungeonRunTimes.FirstOrDefault(x => x.EndTime == null);
        if (dun != null && dun.StartTime < dateTime)
        {
            dun.EndTime = dateTime;
            SetTotalRunTimeInSeconds();
        }
    }

    public void SetTier(Tier tier)
    {
        if ((int) tier <= (int) Tier)
        {
            return;
        }

        Tier = tier;
    }

    private void SetTotalRunTimeInSeconds()
    {
        foreach (var time in DungeonRunTimes.Where(x => x.EndTime != null).ToList())
        {
            TotalRunTimeInSeconds += (int) time.TimeSpan.TotalSeconds;
            DungeonRunTimes.Remove(time);
        }
    }

    private void SetModeAndFaction(MapType mapType)
    {
        switch (mapType)
        {
            case MapType.CorruptedDungeon:
                Faction = Faction.Corrupted;
                Mode = DungeonMode.Corrupted;
                return;
            case MapType.HellGate:
                Faction = Faction.HellGate;
                Mode = DungeonMode.HellGate;
                return;
            case MapType.Expedition:
                Mode = DungeonMode.Expedition;
                return;
            case MapType.RandomDungeon:
                break;
            case MapType.Island:
                break;
            case MapType.Hideout:
                break;
            case MapType.Arena:
                break;
            case MapType.MistsDungeon:
                Faction = Faction.MistsDungeon;
                Mode = DungeonMode.MistsDungeon;
                break;
            case MapType.Mists:
                Faction = Faction.Mists;
                Mode = DungeonMode.Mists;
                break;
            case MapType.Unknown:
            default:
                return;
        }
    }

    public static string TranslationSelectToDelete => LanguageController.Translation("SELECT_TO_DELETE");
    public static string TranslationDungeonFame => LanguageController.Translation("DUNGEON_FAME");
    public static string TranslationDungeonReSpec => LanguageController.Translation("DUNGEON_RESPEC");
    public static string TranslationDungeonSilver => LanguageController.Translation("DUNGEON_SILVER");
    public static string TranslationDungeonFamePerHour => LanguageController.Translation("DUNGEON_FAME_PER_HOUR");
    public static string TranslationDungeonReSpecPerHour => LanguageController.Translation("DUNGEON_RESPEC_PER_HOUR");
    public static string TranslationDungeonSilverPerHour => LanguageController.Translation("DUNGEON_SILVER_PER_HOUR");
    public static string TranslationDungeonRunTime => LanguageController.Translation("DUNGEON_RUN_TIME");
    public static string TranslationNumberOfDungeonFloors => LanguageController.Translation("NUMBER_OF_DUNGEON_FLOORS");
    public static string TranslationExpedition => LanguageController.Translation("EXPEDITION");
    public static string TranslationSolo => LanguageController.Translation("SOLO");
    public static string TranslationStandard => LanguageController.Translation("STANDARD");
    public static string TranslationAvalon => LanguageController.Translation("AVALON");
    public static string TranslationUnknown => LanguageController.Translation("UNKNOWN");
    public static string TranslationFactionFlags => LanguageController.Translation("FACTION_FLAGS");
    public static string TranslationFactionFlagsPerHour => LanguageController.Translation("FACTION_FLAGS_PER_HOUR");
    public static string TranslationFactionCoins => LanguageController.Translation("FACTION_COINS");
    public static string TranslationFactionCoinsPerHour => LanguageController.Translation("FACTION_COINS_PER_HOUR");
    public static string TranslationMight => LanguageController.Translation("MIGHT");
    public static string TranslationMightPerHour => LanguageController.Translation("MIGHT_PER_HOUR");
    public static string TranslationFavor => LanguageController.Translation("FAVOR");
    public static string TranslationFavorPerHour => LanguageController.Translation("FAVOR_PER_HOUR");
    public static string TranslationBestLootedItem => LanguageController.Translation("BEST_LOOTED_ITEM");
    public static string TranslationTotalLootedValue => LanguageController.Translation("TOTAL_LOOT_VALUE");
}