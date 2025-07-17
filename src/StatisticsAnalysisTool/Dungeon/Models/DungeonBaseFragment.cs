using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

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
    private string _mainMapName;
    private bool? _isSelectedForDeletion = false;
    private double _fame;
    private double _reSpec;
    private double _silver;
    private List<ActionInterval> _runningIntervals = new();
    private int _totalRunTimeInSeconds;
    private ObservableCollection<PointOfInterest> _events = new();
    private ObservableCollection<Loot> _loot = new();
    private string _diedName;
    private string _killedBy;
    private MapType _mapType = MapType.Unknown;
    private ClusterType _clusterType = ClusterType.Unknown;
    private double _totalValue;
    private double _famePerHour;
    private double _reSpecPerHour;
    private double _silverPerHour;
    private string _mainMapIndex;
    private Loot _mostValuableLoot;
    private Visibility _mostValuableLootVisibility = Visibility.Collapsed;
    private KillStatus _killStatus;
    private Visibility _itemsContainerVisibility = Visibility.Collapsed;

    public ObservableCollection<Guid> GuidList { get; set; }
    public string DungeonHash => $"{EnterDungeonFirstTime.Ticks}{string.Join(",", GuidList)}";

    protected DungeonBaseFragment(Guid guid, MapType mapType, DungeonMode mode, string mainMapIndex)
    {
        AddTimer(DateTime.UtcNow);
        ClusterType = WorldData.GetClusterTypeByIndex(mainMapIndex);
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
        ClusterType = WorldData.GetClusterTypeByIndex(dto.MainMapIndex);
        MapType = dto.MapType;
        Mode = dto.Mode;
        MainMapIndex = dto.MainMapIndex;
        Faction = dto.Faction;
        Status = DungeonStatus.Done;
        Visibility = Visibility.Visible;
        EnterDungeonFirstTime = dto.EnterDungeonFirstTime;
        RunningIntervals = new List<ActionInterval>() { new (EnterDungeonFirstTime)
        {
            EndTime = EnterDungeonFirstTime.AddSeconds(dto.TotalRunTimeInSeconds)
        }};
        TotalRunTimeInSeconds = dto.TotalRunTimeInSeconds;
        Tier = dto.Tier;
        Fame = dto.Fame;
        Silver = dto.Silver;
        ReSpec = dto.ReSpec;
        KilledBy = dto.KilledBy;
        DiedName = dto.DiedName;
        KillStatus = dto.KillStatus;
        Events = new ObservableCollection<PointOfInterest>(dto.Events.Select(DungeonMapping.Mapping));
        Loot = new ObservableCollection<Loot>(dto.Loot.Select(DungeonMapping.Mapping));

        UpdateTotalSilverValue();
        UpdateMostValuableLoot();
        UpdateMostValuableLootVisibility();
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
            TotalRunTimeInSeconds = GetTotalRunTimeInSeconds();
            FamePerHour = value.GetValuePerHour(GetTotalRunTimeInSeconds());
            OnPropertyChanged();
        }
    }

    public double ReSpec
    {
        get => _reSpec;
        set
        {
            _reSpec = value;
            TotalRunTimeInSeconds = GetTotalRunTimeInSeconds();
            ReSpecPerHour = value.GetValuePerHour(GetTotalRunTimeInSeconds());
            OnPropertyChanged();
        }
    }

    public double Silver
    {
        get => _silver;
        set
        {
            _silver = value;
            TotalRunTimeInSeconds = GetTotalRunTimeInSeconds();
            SilverPerHour = value.GetValuePerHour(GetTotalRunTimeInSeconds());
            UpdateTotalSilverValue();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<PointOfInterest> Events
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

    public KillStatus KillStatus
    {
        get => _killStatus;
        set
        {
            _killStatus = value;
            OnPropertyChanged();
        }
    }

    public List<ActionInterval> RunningIntervals
    {
        get => _runningIntervals;
        set
        {
            _runningIntervals = value;
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

    public ClusterType ClusterType
    {
        get => _clusterType;
        set
        {
            _clusterType = value;
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

    public Loot MostValuableLoot
    {
        get => _mostValuableLoot;
        private set
        {
            _mostValuableLoot = value;
            OnPropertyChanged();
        }
    }

    public Visibility MostValuableLootVisibility
    {
        get => _mostValuableLootVisibility;
        set
        {
            _mostValuableLootVisibility = value;
            OnPropertyChanged();
        }
    }

    public Visibility ItemsContainerVisibility
    {
        get => _itemsContainerVisibility;
        set
        {
            _itemsContainerVisibility = value;
            OnPropertyChanged();
        }
    }

    public int TotalRunTimeInSeconds
    {
        get
        {
            return _totalRunTimeInSeconds;
        }
        private set
        {
            _totalRunTimeInSeconds = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public void UpdateTotalSilverValue()
    {
        var lootValue = Loot?.Sum(x => x.Quantity * FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue) ?? 0;
        TotalValue = Silver + lootValue;
    }

    public void UpdateMostValuableLoot()
    {
        var loot = Loot?.MaxBy(x => x?.EstimatedMarketValueInternal) ?? new Loot();
        MostValuableLoot = loot;
    }

    public void UpdateMostValuableLootVisibility()
    {
        MostValuableLootVisibility = MostValuableLoot is not null && MostValuableLoot.EstimatedMarketValue.DoubleValue > 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public void AddTimer(DateTime time)
    {
        if (RunningIntervals.Any(x => x.EndTime == null))
        {
            var dun = RunningIntervals.FirstOrDefault(x => x.EndTime == null);
            if (dun != null)
            {
                dun.EndTime = time;
                TotalRunTimeInSeconds = GetTotalRunTimeInSeconds();
                RunningIntervals.Add(new ActionInterval(time));
            }
        }
        else
        {
            RunningIntervals.Add(new ActionInterval(time));
        }
    }

    public void EndTimer()
    {
        var dateTime = DateTime.UtcNow;

        var dun = RunningIntervals.FirstOrDefault(x => x.EndTime == null);
        if (dun != null && dun.StartTime < dateTime)
        {
            dun.EndTime = dateTime;
            TotalRunTimeInSeconds = GetTotalRunTimeInSeconds();
        }
    }

    private int GetTotalRunTimeInSeconds()
    {
        int newTotalRunTime = 0;

        foreach (var time in RunningIntervals.Where(x => x.EndTime != null).ToList())
        {
            newTotalRunTime += (int) time.TimeSpan.TotalSeconds;
        }

        var currentlyRunningTime = RunningIntervals.FirstOrDefault(x => x.EndTime == null);
        if (currentlyRunningTime != null)
        {
            newTotalRunTime += (int) (DateTime.UtcNow - currentlyRunningTime.StartTime).TotalSeconds;
        }

        return newTotalRunTime;
    }

    public void SetTier(Tier tier)
    {
        if ((int) tier <= (int) Tier)
        {
            return;
        }

        Tier = tier;
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

    private void PerformShowLootedItems(object value)
    {
        if (Loot?.Count <= 0)
        {
            return;
        }

        ItemsContainerVisibility = ItemsContainerVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private ICommand _showLootedItems;
    public ICommand ShowLootedItems => _showLootedItems ??= new CommandHandler(PerformShowLootedItems, true);

    public static string TranslationSelectToDelete => LocalizationController.Translation("SELECT_TO_DELETE");
    public static string TranslationFame => LocalizationController.Translation("FAME");
    public static string TranslationReSpec => LocalizationController.Translation("RESPEC");
    public static string TranslationSilver => LocalizationController.Translation("SILVER");
    public static string TranslationFamePerHour => LocalizationController.Translation("FAME_PER_HOUR");
    public static string TranslationReSpecPerHour => LocalizationController.Translation("RESPEC_PER_HOUR");
    public static string TranslationSilverPerHour => LocalizationController.Translation("SILVER_PER_HOUR");
    public static string TranslationRunTime => LocalizationController.Translation("RUN_TIME");
    public static string TranslationNumberOfDungeonFloors => LocalizationController.Translation("NUMBER_OF_DUNGEON_FLOORS");
    public static string TranslationExpedition => LocalizationController.Translation("EXPEDITION");
    public static string TranslationSolo => LocalizationController.Translation("SOLO");
    public static string TranslationStandard => LocalizationController.Translation("STANDARD");
    public static string TranslationAvalon => LocalizationController.Translation("AVALON");
    public static string TranslationUnknown => LocalizationController.Translation("UNKNOWN");
    public static string TranslationFactionFlags => LocalizationController.Translation("FACTION_FLAGS");
    public static string TranslationFactionFlagsPerHour => LocalizationController.Translation("FACTION_FLAGS_PER_HOUR");
    public static string TranslationFactionCoins => LocalizationController.Translation("FACTION_COINS");
    public static string TranslationFactionCoinsPerHour => LocalizationController.Translation("FACTION_COINS_PER_HOUR");
    public static string TranslationMight => LocalizationController.Translation("MIGHT");
    public static string TranslationMightPerHour => LocalizationController.Translation("MIGHT_PER_HOUR");
    public static string TranslationFavor => LocalizationController.Translation("FAVOR");
    public static string TranslationFavorPerHour => LocalizationController.Translation("FAVOR_PER_HOUR");
    public static string TranslationBestLootedItem => LocalizationController.Translation("BEST_LOOTED_ITEM");
    public static string TranslationTotalLootedValue => LocalizationController.Translation("TOTAL_LOOT_VALUE");
    public static string TranslationClusterType => LocalizationController.Translation("CLUSTER_TYPE");
    public static string TranslationMostValuableLoot => LocalizationController.Translation("MOST_VALUABLE_LOOT");
    public static string TranslationCorrupted => LocalizationController.Translation("CORRUPTED");
    public static string TranslationHellGate => LocalizationController.Translation("HELLGATE");
    public static string TranslationMists => LocalizationController.Translation("MISTS");
    public static string TranslationMistsDungeon => LocalizationController.Translation("MISTS_DUNGEON");
    public static string TranslationKilledBy => LocalizationController.Translation("KILLED_BY");
    public static string TranslationAbyssalDepths => LocalizationController.Translation("ABYSSALDEPTHS");
}