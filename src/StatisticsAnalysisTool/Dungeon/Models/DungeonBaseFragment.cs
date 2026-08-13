using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace StatisticsAnalysisTool.Dungeon.Models;

public abstract class DungeonBaseFragment : BaseViewModel
{
    private const int AlwaysVisibleChestCount = 3;
    private ObservableCollection<PointOfInterest> _events = [];
    private ObservableCollection<Loot> _loot = [];
    private readonly HashSet<PointOfInterest> _subscribedEvents = [];
    private IReadOnlyList<DungeonLootGroup> _chestLootGroups = [];
    private DungeonLootGroup _otherLootGroup = DungeonLootGroup.CreateOtherLoot([], false);
    private bool _areAdditionalChestsVisible;

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
        _loot.CollectionChanged += OnLootCollectionChanged;
        _events.CollectionChanged += OnEventsCollectionChanged;
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
        RunningIntervals =
        [
            new(EnterDungeonFirstTime)
            {
                EndTime = EnterDungeonFirstTime.AddSeconds(dto.TotalRunTimeInSeconds)
            }
        ];
        TotalRunTimeInSeconds = dto.TotalRunTimeInSeconds;
        Tier = dto.Tier;
        Fame = dto.Fame;
        Silver = dto.Silver;
        ReSpec = dto.ReSpec;
        KilledBy = dto.KilledBy;
        DiedName = dto.DiedName;
        KillStatus = dto.KillStatus;
        PartySize = Math.Max(1, dto.PartySize);
        Events = new ObservableCollection<PointOfInterest>(dto.Events.Select(DungeonMapping.Mapping));
        Loot = new ObservableCollection<Loot>(dto.Loot.Select(DungeonMapping.Mapping));

        UpdateTotalSilverValue();
        UpdateMostValuableLoot();
        UpdateMostValuableLootVisibility();
        RebuildLootPresentation();
    }

    public int Count
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public MapType MapType
    {
        get;
        set
        {
            field = value;
            SetModeAndFaction(field);
            OnPropertyChanged();
        }
    }

    public Faction Faction
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Faction.Unknown;

    public DungeonStatus Status
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility Visibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public DateTime EnterDungeonFirstTime
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public DungeonMode Mode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Tier Tier
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Tier.Unknown;

    public string MainMapIndex
    {
        get;
        set
        {
            field = value;
            MainMapName = WorldData.GetUniqueNameOrDefault(value);
            OnPropertyChanged();
        }
    }

    public bool? IsSelectedForDeletion
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = false;

    public double Fame
    {
        get;
        set
        {
            field = value;
            TotalRunTimeInSeconds = GetTotalRunTimeInSeconds();
            FamePerHour = value.GetValuePerHour(GetTotalRunTimeInSeconds());
            OnPropertyChanged();
        }
    }

    public double ReSpec
    {
        get;
        set
        {
            field = value;
            TotalRunTimeInSeconds = GetTotalRunTimeInSeconds();
            ReSpecPerHour = value.GetValuePerHour(GetTotalRunTimeInSeconds());
            OnPropertyChanged();
        }
    }

    public double Silver
    {
        get;
        set
        {
            field = value;
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
            _events.CollectionChanged -= OnEventsCollectionChanged;
            UnsubscribeFromEvents();
            _events = value ?? [];
            _events.CollectionChanged += OnEventsCollectionChanged;
            SubscribeToEvents();
            OnPropertyChanged();
            RebuildLootPresentation();
        }
    }

    public ObservableCollection<Loot> Loot
    {
        get => _loot;
        set
        {
            _loot.CollectionChanged -= OnLootCollectionChanged;
            _loot = value ?? [];
            _loot.CollectionChanged += OnLootCollectionChanged;
            OnPropertyChanged();
            OnPropertyChanged(nameof(VisibleLoot));
            OnPropertyChanged(nameof(HasLootVisibility));
            OnPropertyChanged(nameof(LootSummaryText));
            RebuildLootPresentation();
        }
    }

    public string DiedName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string KilledBy
    {
        get;
        set
        {
            field = value;
            KilledByVisibility = string.IsNullOrEmpty(KilledBy) ? Visibility.Hidden : Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public Visibility KilledByVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Visible;

    public KillStatus KillStatus
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int PartySize
    {
        get;
        set
        {
            field = Math.Max(1, value);
            OnPropertyChanged();
        }
    } = 1;

    public List<ActionInterval> RunningIntervals
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    #region Composite values that are not in the DTO

    public string MainMapName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ClusterType ClusterType
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double TotalValue
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double FamePerHour
    {
        get
        {
            if (double.IsNaN(field))
            {
                return 0;
            }

            return field;
        }
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double ReSpecPerHour
    {
        get
        {
            if (double.IsNaN(field))
            {
                return 0;
            }

            return field;
        }
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public double SilverPerHour
    {
        get
        {
            if (double.IsNaN(field))
            {
                return 0;
            }

            return field;
        }
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Loot MostValuableLoot
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Visibility MostValuableLootVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public Visibility ItemsContainerVisibility
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LootToggleToolTip));
        }
    } = Visibility.Collapsed;

    public IEnumerable<Loot> VisibleLoot => Loot.Where(IsLootVisible);
    public Visibility HasLootVisibility => VisibleLoot.Any() ? Visibility.Visible : Visibility.Collapsed;
    public string LootSummaryText => string.Format(CultureInfo.CurrentCulture, TranslationLootEntries, VisibleLoot.Count());
    public string LootToggleToolTip => ItemsContainerVisibility == Visibility.Visible ? TranslationHideCollectedLoot : TranslationShowCollectedLoot;

    public int TotalRunTimeInSeconds
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public int EffectiveRunTimeInSeconds => Math.Max(TotalRunTimeInSeconds, GetTotalRunTimeInSeconds());

    #endregion

    public void UpdateTotalSilverValue()
    {
        var lootValue = VisibleLoot.Sum(x => x.Quantity * FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
        TotalValue = Silver + lootValue;
        OnPropertyChanged(nameof(HasLootVisibility));
        OnPropertyChanged(nameof(LootSummaryText));
        OnPropertyChanged(nameof(LootToggleToolTip));
    }

    private void OnLootCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasLootVisibility));
        OnPropertyChanged(nameof(VisibleLoot));
        UpdateTotalSilverValue();
        UpdateMostValuableLoot();
        UpdateMostValuableLootVisibility();
        OnPropertyChanged(nameof(LootSummaryText));
        RebuildLootPresentation();
    }

    public void RefreshLootVisibility()
    {
        var hasVisibleLoot = VisibleLoot.Any();
        if (!hasVisibleLoot)
        {
            ItemsContainerVisibility = Visibility.Collapsed;
        }

        OnPropertyChanged(nameof(VisibleLoot));
        UpdateTotalSilverValue();
        UpdateMostValuableLoot();
        UpdateMostValuableLootVisibility();
        OnPropertyChanged(nameof(HasLootVisibility));
        OnPropertyChanged(nameof(LootSummaryText));
        RebuildLootPresentation();
    }

    private static bool IsLootVisible(Loot loot)
    {
        return loot.SourceType != DungeonLootSourceType.Player
               || SettingsController.CurrentSettings.IsDungeonPlayerLootVisible;
    }

    public void UpdateMostValuableLoot()
    {
        var loot = VisibleLoot.MaxBy(x => x?.EstimatedMarketValueInternal) ?? new Loot();
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
            case MapType.DragonArea:
                Faction = Faction.DragonArea;
                Mode = DungeonMode.DragonArea;
                break;
            case MapType.Unknown:
            default:
                return;
        }
    }

    private void PerformShowLootedItems(object value)
    {
        if (!VisibleLoot.Any())
        {
            return;
        }

        ItemsContainerVisibility = ItemsContainerVisibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    public ICommand ShowLootedItems => field ??= new CommandHandler(PerformShowLootedItems, true);

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
    public static string TranslationSoloDungeon => LocalizationController.Translation("SOLO_DUNGEON");
    public static string TranslationStandardDungeon => LocalizationController.Translation("STANDARD_DUNGEON");
    public static string TranslationAvalonianDungeon => LocalizationController.Translation("AVALONIAN_DUNGEON");
    public static string TranslationCorruptedDungeon => LocalizationController.Translation("CORRUPTED_DUNGEON");
    public static string TranslationFactionStanding => LocalizationController.Translation("FACTION_STANDING");
    public static string TranslationFactionStandingPerHour => LocalizationController.Translation("FACTION_STANDING_PER_HOUR");
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
    public static string TranslationDragonArea => LocalizationController.Translation("DRAGONAREA");
    public static string TranslationLootEntries => LocalizationController.Translation("LOOT_ENTRIES");
    public static string TranslationShowCollectedLoot => LocalizationController.Translation("SHOW_COLLECTED_LOOT");
    public static string TranslationHideCollectedLoot => LocalizationController.Translation("HIDE_COLLECTED_LOOT");
    public static string TranslationTotalValue => LocalizationController.Translation("TOTAL_VALUE");
    public static string TranslationOtherLoot => LocalizationController.Translation("OTHER_LOOT");
    public static string TranslationMoreChests => LocalizationController.Translation("MORE_CHESTS");
    public static string TranslationShowFewerChests => LocalizationController.Translation("SHOW_FEWER_CHESTS");
    public static string TranslationBrecilianStanding => LocalizationController.Translation("BRECILIAN_STANDING");
    public static string TranslationPlayers => LocalizationController.Translation("PLAYERS");
    public string ContentDisplayName => Mode switch
    {
        DungeonMode.Solo => TranslationSoloDungeon,
        DungeonMode.Standard => TranslationStandardDungeon,
        DungeonMode.Avalon => TranslationAvalonianDungeon,
        DungeonMode.HellGate => TranslationHellGate,
        DungeonMode.Corrupted => TranslationCorruptedDungeon,
        DungeonMode.Expedition => TranslationExpedition,
        DungeonMode.Mists => TranslationMists,
        DungeonMode.MistsDungeon => TranslationMistsDungeon,
        DungeonMode.AbyssalDepths => TranslationAbyssalDepths,
        DungeonMode.DragonArea => TranslationDragonArea,
        _ => TranslationUnknown
    };

    public string ContentIconPath => this switch
    {
        RandomDungeonFragment { Mode: DungeonMode.Solo } => "/Assets/MiniMapMarker/solo_dungeon.png",
        RandomDungeonFragment { Mode: DungeonMode.Standard } => "/Assets/MiniMapMarker/group_dungeon.png",
        RandomDungeonFragment { Mode: DungeonMode.Avalon } => "/Assets/MiniMapMarker/raid_dungeon.png",
        MistsFragment { Rarity: MistsRarity.Uncommon } => "/Assets/shiny_uncommon.png",
        MistsFragment { Rarity: MistsRarity.Rare } => "/Assets/shiny_rare.png",
        MistsFragment { Rarity: MistsRarity.Epic } => "/Assets/shiny_epic.png",
        MistsFragment { Rarity: MistsRarity.Legendary } => "/Assets/shiny_legendary.png",
        MistsFragment => "/Assets/shiny_common.png",
        MistsDungeonFragment => "/Assets/mists_dungeon.png",
        HellGateFragment => "/Assets/hellgate.png",
        CorruptedFragment => "/Assets/corrupted_dungeon.png",
        AbyssalDepthsFragment => "/Assets/abyssal_depths.png",
        _ => "/Assets/dungeon.png"
    };

    public string TierDisplayText => Tier == Tier.Unknown ? string.Empty : $"T{(int) Tier}";
    public Visibility TierBadgeVisibility => Tier == Tier.Unknown ? Visibility.Collapsed : Visibility.Visible;
    public string EnchantmentDisplayText => this is RandomDungeonFragment { Level: >= 0 } dungeon ? $".{dungeon.Level}" : string.Empty;
    public Visibility EnchantmentBadgeVisibility => this is RandomDungeonFragment { Level: >= 0 } ? Visibility.Visible : Visibility.Collapsed;
    public Visibility MapNameVisibility => string.IsNullOrWhiteSpace(MainMapName) ? Visibility.Collapsed : Visibility.Visible;
    public Visibility KillInformationVisibility => KillStatus == KillStatus.Unknown ? Visibility.Collapsed : Visibility.Visible;

    public int FloorCount => this switch
    {
        RandomDungeonFragment dungeon => dungeon.NumberOfFloors,
        AbyssalDepthsFragment dungeon => dungeon.NumberOfFloors,
        DragonAreaFragment dungeon => dungeon.NumberOfFloors,
        _ => 0
    };

    public Visibility FloorCountVisibility => FloorCount > 0 ? Visibility.Visible : Visibility.Collapsed;
    public IEnumerable<CheckPoint> DisplayedCheckPoints => this is ExpeditionFragment dungeon ? dungeon.CheckPoints : [];
    public Visibility CheckPointVisibility => DisplayedCheckPoints.Any() ? Visibility.Visible : Visibility.Collapsed;
    public IReadOnlyList<DungeonRunMetric> PerformanceMetrics => DungeonRunPresentationService.BuildMetrics(this);
    private IEnumerable<DungeonLootGroup> DisplayedChestLootGroups => AreAdditionalChestsVisible ? ChestLootGroups : ChestLootGroups.Take(AlwaysVisibleChestCount);
    public IEnumerable<DungeonLootGroup> DisplayedLootGroups => OtherLootGroup.Items.Count > 0
        ? DisplayedChestLootGroups.Append(OtherLootGroup)
        : DisplayedChestLootGroups;
    public IReadOnlyList<DungeonLootGroup> ChestLootGroups => _chestLootGroups;
    public DungeonLootGroup OtherLootGroup => _otherLootGroup;
    public int HiddenChestCount => Math.Max(0, ChestLootGroups.Count - AlwaysVisibleChestCount);
    public Visibility LootGroupVisibility => ChestLootGroups.Count > 0 || OtherLootGroup.Items.Count > 0
        ? Visibility.Visible
        : Visibility.Collapsed;
    public Visibility AdditionalChestToggleVisibility => HiddenChestCount > 0 ? Visibility.Visible : Visibility.Collapsed;

    public bool AreAdditionalChestsVisible
    {
        get => _areAdditionalChestsVisible;
        private set
        {
            if (_areAdditionalChestsVisible == value)
            {
                return;
            }

            _areAdditionalChestsVisible = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayedLootGroups));
            OnPropertyChanged(nameof(AdditionalChestToggleText));
        }
    }

    public string AdditionalChestToggleText => AreAdditionalChestsVisible
        ? TranslationShowFewerChests
        : string.Format(CultureInfo.CurrentCulture, TranslationMoreChests, HiddenChestCount);

    private void OnEventsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (var pointOfInterest in e.OldItems.OfType<PointOfInterest>())
            {
                UnsubscribeFromEvent(pointOfInterest);
            }
        }

        if (e.NewItems is not null)
        {
            foreach (var pointOfInterest in e.NewItems.OfType<PointOfInterest>())
            {
                SubscribeToEvent(pointOfInterest);
            }
        }

        RebuildLootPresentation();
    }

    private void SubscribeToEvents()
    {
        foreach (var pointOfInterest in Events)
        {
            SubscribeToEvent(pointOfInterest);
        }
    }

    private void SubscribeToEvent(PointOfInterest pointOfInterest)
    {
        if (!_subscribedEvents.Add(pointOfInterest))
        {
            return;
        }

        pointOfInterest.PropertyChanged += OnEventPropertyChanged;
    }

    private void UnsubscribeFromEvents()
    {
        foreach (var pointOfInterest in _subscribedEvents.ToList())
        {
            UnsubscribeFromEvent(pointOfInterest);
        }
    }

    private void UnsubscribeFromEvent(PointOfInterest pointOfInterest)
    {
        if (!_subscribedEvents.Remove(pointOfInterest))
        {
            return;
        }

        pointOfInterest.PropertyChanged -= OnEventPropertyChanged;
    }

    private void OnEventPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PointOfInterest.Status)
            or nameof(PointOfInterest.Type)
            or nameof(PointOfInterest.Rarity)
            or nameof(PointOfInterest.IsBossChest))
        {
            RebuildLootPresentation();
        }
    }

    private void RebuildLootPresentation()
    {
        var expandedChestIds = _chestLootGroups.Where(x => x.IsExpanded).Select(x => x.SourceObjectId).ToHashSet();
        var presentation = DungeonRunPresentationService.BuildLootPresentation(
            Events,
            VisibleLoot,
            expandedChestIds,
            _otherLootGroup.IsExpanded);
        _chestLootGroups = presentation.ChestGroups;
        _otherLootGroup = presentation.OtherLootGroup;
        if (_chestLootGroups.Count <= AlwaysVisibleChestCount)
        {
            _areAdditionalChestsVisible = false;
            ItemsContainerVisibility = Visibility.Collapsed;
        }

        OnPropertyChanged(nameof(ChestLootGroups));
        OnPropertyChanged(nameof(DisplayedLootGroups));
        OnPropertyChanged(nameof(OtherLootGroup));
        OnPropertyChanged(nameof(HiddenChestCount));
        OnPropertyChanged(nameof(LootGroupVisibility));
        OnPropertyChanged(nameof(AdditionalChestToggleVisibility));
        OnPropertyChanged(nameof(AdditionalChestToggleText));
    }
    private void PerformToggleAdditionalChests(object value)
    {
        AreAdditionalChestsVisible = !AreAdditionalChestsVisible;
        ItemsContainerVisibility = AreAdditionalChestsVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    private ICommand _toggleAdditionalChests;
    public ICommand ToggleAdditionalChests => _toggleAdditionalChests ??= new CommandHandler(PerformToggleAdditionalChests, true);


    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(Fame):
            case nameof(FamePerHour):
            case nameof(ReSpec):
            case nameof(ReSpecPerHour):
            case nameof(Silver):
            case nameof(SilverPerHour):
            case nameof(TotalValue):
            case "Might":
            case "MightPerHour":
            case "Favor":
            case "FavorPerHour":
            case "FactionCoins":
            case "FactionCoinsPerHour":
            case "FactionStanding":
            case "FactionStandingPerHour":
            case "BrecilianStanding":
            case "BrecilianStandingPerHour":
                base.OnPropertyChanged(nameof(PerformanceMetrics));
                break;
            case nameof(Mode):
                base.OnPropertyChanged(nameof(ContentDisplayName));
                base.OnPropertyChanged(nameof(ContentIconPath));
                break;
            case nameof(Tier):
                base.OnPropertyChanged(nameof(TierDisplayText));
                base.OnPropertyChanged(nameof(TierBadgeVisibility));
                break;
            case nameof(MainMapName):
                base.OnPropertyChanged(nameof(MapNameVisibility));
                break;
            case nameof(KillStatus):
                base.OnPropertyChanged(nameof(KillInformationVisibility));
                break;
            case "Level":
                base.OnPropertyChanged(nameof(EnchantmentDisplayText));
                base.OnPropertyChanged(nameof(EnchantmentBadgeVisibility));
                break;
            case "Rarity":
                base.OnPropertyChanged(nameof(ContentIconPath));
                break;
            case "NumberOfFloors":
                base.OnPropertyChanged(nameof(FloorCount));
                base.OnPropertyChanged(nameof(FloorCountVisibility));
                break;
            case "CheckPoints":
                base.OnPropertyChanged(nameof(DisplayedCheckPoints));
                base.OnPropertyChanged(nameof(CheckPointVisibility));
                break;
        }
    }
}
