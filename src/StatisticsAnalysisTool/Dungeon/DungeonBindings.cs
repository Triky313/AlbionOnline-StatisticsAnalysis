using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonBindings : BaseViewModel
{
    private CancellationTokenSource _filterCancellationTokenSource;
    private ObservableRangeCollection<DungeonBaseFragment> _dungeons = new();
    private ListCollectionView _dungeonsCollectionView;
    private DungeonCloseTimer _dungeonCloseTimer = new();
    private DungeonStatsFilter _dungeonStatsFilter;
    private DungeonStats _stats = new();
    private GridLength _gridSplitterPosition;
    private DungeonsTranslation _translation = new();
    private DashboardChartRangeOption _selectedStatsTimeType;
    private DungeonStats _dungeonStatsSelection;
    private DungeonOptionsObject _dungeonOptionsObject = new();
    private StatsTypeFilterStruct _selectedDungeonStatsType;

    public DungeonBindings()
    {
        DungeonStatsFilter = new DungeonStatsFilter(this);
        DungeonOptionsObject.PlayerLootVisibilityChanged += OnPlayerLootVisibilityChanged;
        RefreshLocalization();
    }

    public ObservableRangeCollection<DungeonBaseFragment> Dungeons
    {
        get => _dungeons;
        set
        {
            _dungeons = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView DungeonsCollectionView
    {
        get => _dungeonsCollectionView;
        set
        {
            _dungeonsCollectionView = value;
            OnPropertyChanged();
        }
    }

    public DungeonCloseTimer DungeonCloseTimer
    {
        get => _dungeonCloseTimer;
        set
        {
            _dungeonCloseTimer = value;
            OnPropertyChanged();
        }
    }

    public DungeonStats Stats
    {
        get => _stats;
        set
        {
            _stats = value;
            OnPropertyChanged();
        }
    }

    public DungeonAnalytics Analytics { get; } = new();

    public DungeonStatsFilter DungeonStatsFilter
    {
        get => _dungeonStatsFilter;
        set
        {
            _dungeonStatsFilter = value;
            OnPropertyChanged();
        }
    }

    public DungeonOptionsObject DungeonOptionsObject
    {
        get => _dungeonOptionsObject;
        set
        {
            _dungeonOptionsObject = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.DungeonsGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<DashboardChartRangeOption> DungeonStatTimeTypes
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public DashboardChartRangeOption SelectedStatsTimeType
    {
        get => _selectedStatsTimeType;
        set
        {
            if (value == null || ReferenceEquals(_selectedStatsTimeType, value))
            {
                return;
            }

            _selectedStatsTimeType = value;
            OnPropertyChanged();
            _ = UpdateFilteredDungeonsAsync();
        }
    }

    public IReadOnlyList<StatsTypeFilterStruct> DungeonStatsType
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public StatsTypeFilterStruct SelectedDungeonStatsType
    {
        get => _selectedDungeonStatsType;
        set
        {
            if (_selectedDungeonStatsType.StatsViewType == value.StatsViewType)
            {
                return;
            }

            _selectedDungeonStatsType = value;
            UpdateFilterAvailability();
            UpdateStatsView();
            OnPropertyChanged();
            _ = UpdateFilteredDungeonsAsync();
        }
    }

    public DungeonStats DungeonStatsSelection
    {
        get => _dungeonStatsSelection;
        set
        {
            _dungeonStatsSelection = value;
            OnPropertyChanged();
        }
    }

    public bool IsTierFilterEnabled
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsEnchantmentFilterEnabled
    {
        get;
        private set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public DungeonsTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    public void InitListCollectionView()
    {
        DungeonsCollectionView = CreateCollectionView(Dungeons);
        _ = UpdateFilteredDungeonsAsync();
    }

    private void OnPlayerLootVisibilityChanged(object sender, EventArgs e)
    {
        foreach (var dungeon in Dungeons)
        {
            dungeon.RefreshLootVisibility();
        }

        _ = UpdateFilteredDungeonsAsync();
    }

    public void RefreshLocalization()
    {
        var selectedRange = SelectedStatsTimeType;
        var selectedMode = SelectedDungeonStatsType.StatsViewType;

        DungeonStatTimeTypes = DashboardChartRangeOption.CreateDefault().Skip(1).ToList();
        DungeonStatsType = CreateContentTabs();

        _selectedStatsTimeType = selectedRange is null
            ? DungeonStatTimeTypes[0]
            : DungeonStatTimeTypes.FirstOrDefault(x => x.BucketCount == selectedRange.BucketCount && x.Unit == selectedRange.Unit)
              ?? DungeonStatTimeTypes[0];
        _selectedDungeonStatsType = DungeonStatsType.FirstOrDefault(x => x.StatsViewType == selectedMode);
        Translation = new DungeonsTranslation();
        UpdateFilterAvailability();

        Analytics.RefreshLocalization();
        DungeonOptionsObject.RefreshLocalization();
        foreach (var dungeon in Dungeons)
        {
            dungeon.RefreshLootVisibility();
        }

        OnPropertyChanged(null);
        _ = UpdateFilteredDungeonsAsync();
    }

    public async Task UpdateFilteredDungeonsAsync()
    {
        _filterCancellationTokenSource?.Cancel();
        _filterCancellationTokenSource?.Dispose();
        _filterCancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _filterCancellationTokenSource.Token;

        var dungeons = Dungeons?.ToList() ?? [];
        var selectedMode = SelectedDungeonStatsType.StatsViewType;
        var selectedTiers = DungeonStatsFilter?.TierFilters.ToHashSet() ?? [];
        var selectedLevels = DungeonStatsFilter?.LevelFilters.ToHashSet() ?? [];
        var selectedRange = SelectedStatsTimeType ?? DungeonStatTimeTypes.FirstOrDefault();
        if (selectedRange == null)
        {
            return;
        }

        var rangeDuration = GetDuration(selectedRange);
        var currentRangeEnd = DateTime.UtcNow;
        var currentRangeStart = currentRangeEnd.Subtract(rangeDuration);
        var previousRangeStart = currentRangeStart.Subtract(rangeDuration);
        var appliesTierFilter = SupportsTier(selectedMode);
        var appliesEnchantmentFilter = SupportsEnchantment(selectedMode);

        try
        {
            var filteredPeriods = await Task.Run(() =>
            {
                var current = new List<DungeonBaseFragment>();
                var previous = new List<DungeonBaseFragment>();

                foreach (var dungeon in dungeons)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!MatchesMetadata(dungeon, selectedMode, selectedTiers, selectedLevels, appliesTierFilter, appliesEnchantmentFilter))
                    {
                        continue;
                    }

                    if (dungeon.EnterDungeonFirstTime >= currentRangeStart && dungeon.EnterDungeonFirstTime <= currentRangeEnd)
                    {
                        current.Add(dungeon);
                    }
                    else if (dungeon.EnterDungeonFirstTime >= previousRangeStart && dungeon.EnterDungeonFirstTime < currentRangeStart)
                    {
                        previous.Add(dungeon);
                    }
                }

                current.Sort((left, right) => right.EnterDungeonFirstTime.CompareTo(left.EnterDungeonFirstTime));
                return new FilteredDungeonPeriods(current, previous);
            }, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await ApplyFilteredPeriodsAsync(filteredPeriods, selectedRange, selectedMode, cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }

    public List<DungeonBaseFragment> ParallelDungeonFilterProcess()
    {
        var selectedMode = SelectedDungeonStatsType.StatsViewType;
        var selectedTiers = DungeonStatsFilter.TierFilters.ToHashSet();
        var selectedLevels = DungeonStatsFilter.LevelFilters.ToHashSet();
        var rangeDuration = GetDuration(SelectedStatsTimeType);
        var rangeStart = DateTime.UtcNow.Subtract(rangeDuration);

        return Dungeons
            .Where(dungeon => dungeon.EnterDungeonFirstTime >= rangeStart)
            .Where(dungeon => MatchesMetadata(
                dungeon,
                selectedMode,
                selectedTiers,
                selectedLevels,
                SupportsTier(selectedMode),
                SupportsEnchantment(selectedMode)))
            .OrderByDescending(x => x.EnterDungeonFirstTime)
            .ToList();
    }

    private async Task ApplyFilteredPeriodsAsync(
        FilteredDungeonPeriods filteredPeriods,
        DashboardChartRangeOption selectedRange,
        DungeonMode selectedMode,
        CancellationToken cancellationToken)
    {
        void Apply()
        {
            cancellationToken.ThrowIfCancellationRequested();
            DungeonsCollectionView = CreateCollectionView(filteredPeriods.Current);
            Stats.Set(filteredPeriods.Current);
            UpdateStatsView();
            Analytics.Update(filteredPeriods.Current, filteredPeriods.Previous, GetComparisonText(selectedRange), selectedMode);
        }

        if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            await dispatcher.InvokeAsync(Apply);
            return;
        }

        Apply();
    }

    private static ListCollectionView CreateCollectionView(IEnumerable<DungeonBaseFragment> dungeons)
    {
        var collectionView = CollectionViewSource.GetDefaultView(dungeons.ToList()) as ListCollectionView;
        if (collectionView != null)
        {
            collectionView.IsLiveSorting = true;
            collectionView.IsLiveFiltering = true;
            collectionView.CustomSort = new DungeonComparer();
            collectionView.Refresh();
        }

        return collectionView;
    }

    private static bool MatchesMetadata(
        DungeonBaseFragment dungeon,
        DungeonMode selectedMode,
        IReadOnlySet<Tier> selectedTiers,
        IReadOnlySet<ItemLevel> selectedLevels,
        bool appliesTierFilter,
        bool appliesEnchantmentFilter)
    {
        if (selectedMode != DungeonMode.Unknown && dungeon.Mode != selectedMode)
        {
            return false;
        }

        if (appliesTierFilter && !selectedTiers.Contains(dungeon.Tier))
        {
            return false;
        }

        if (appliesEnchantmentFilter
            && dungeon is RandomDungeonFragment randomDungeon
            && !selectedLevels.Contains((ItemLevel) randomDungeon.Level))
        {
            return false;
        }

        return true;
    }

    private static TimeSpan GetDuration(DashboardChartRangeOption range)
    {
        return range.Unit switch
        {
            DashboardChartRangeUnit.Minute => TimeSpan.FromMinutes(range.BucketCount),
            DashboardChartRangeUnit.Hour => TimeSpan.FromHours(range.BucketCount),
            DashboardChartRangeUnit.Day => TimeSpan.FromDays(range.BucketCount),
            _ => TimeSpan.FromHours(1)
        };
    }

    private static string GetComparisonText(DashboardChartRangeOption range)
    {
        return range.Unit switch
        {
            DashboardChartRangeUnit.Minute => LocalizationController.Translation("VS_PREVIOUS_MINUTES"),
            DashboardChartRangeUnit.Hour when range.BucketCount == 1 => LocalizationController.Translation("VS_PREVIOUS_HOUR"),
            DashboardChartRangeUnit.Hour => LocalizationController.Translation("VS_PREVIOUS_HOURS"),
            DashboardChartRangeUnit.Day when range.BucketCount == 1 => LocalizationController.Translation("VS_PREVIOUS_DAY"),
            DashboardChartRangeUnit.Day => LocalizationController.Translation("VS_PREVIOUS_DAYS"),
            _ => string.Empty
        };
    }

    private void UpdateFilterAvailability()
    {
        var selectedMode = SelectedDungeonStatsType.StatsViewType;
        IsTierFilterEnabled = SupportsTier(selectedMode);
        IsEnchantmentFilterEnabled = SupportsEnchantment(selectedMode);
    }

    private static bool SupportsTier(DungeonMode mode)
    {
        return mode is DungeonMode.Unknown
            or DungeonMode.Solo
            or DungeonMode.Standard
            or DungeonMode.Avalon
            or DungeonMode.Mists
            or DungeonMode.MistsDungeon
            or DungeonMode.AbyssalDepths;
    }

    private static bool SupportsEnchantment(DungeonMode mode)
    {
        return mode is DungeonMode.Unknown or DungeonMode.Solo or DungeonMode.Standard or DungeonMode.Avalon;
    }

    private static IReadOnlyList<StatsTypeFilterStruct> CreateContentTabs()
    {
        return
        [
            CreateContentTab("OVERVIEW", DungeonMode.Unknown),
            CreateContentTab("SOLO_DUNGEON", DungeonMode.Solo),
            CreateContentTab("STANDARD_DUNGEON", DungeonMode.Standard),
            CreateContentTab("AVALONIAN_DUNGEON", DungeonMode.Avalon),
            CreateContentTab("CORRUPTED", DungeonMode.Corrupted),
            CreateContentTab("HELLGATE", DungeonMode.HellGate),
            CreateContentTab("HCE_EXPEDITION", DungeonMode.Expedition),
            CreateContentTab("MISTS", DungeonMode.Mists),
            CreateContentTab("MISTS_DUNGEON", DungeonMode.MistsDungeon),
            CreateContentTab("ABYSSALDEPTHS", DungeonMode.AbyssalDepths),
            CreateContentTab("DRAGONAREA", DungeonMode.DragonArea)
        ];
    }

    private static StatsTypeFilterStruct CreateContentTab(string translationKey, DungeonMode dungeonMode)
    {
        return new StatsTypeFilterStruct()
        {
            Name = LocalizationController.Translation(translationKey),
            StatsViewType = dungeonMode
        };
    }

    private void UpdateStatsView()
    {
        DungeonStatsSelection = SelectedDungeonStatsType.StatsViewType switch
        {
            DungeonMode.Unknown => new DungeonStats { StatsTotal = Stats.StatsTotal },
            DungeonMode.Solo => new DungeonStats { StatsSolo = Stats.StatsSolo },
            DungeonMode.Standard => new DungeonStats { StatsStandard = Stats.StatsStandard },
            DungeonMode.Avalon => new DungeonStats { StatsAvalonian = Stats.StatsAvalonian },
            DungeonMode.Corrupted => new DungeonStats { StatsCorrupted = Stats.StatsCorrupted },
            DungeonMode.HellGate => new DungeonStats { StatsHellGate = Stats.StatsHellGate },
            DungeonMode.Expedition => new DungeonStats { StatsExpedition = Stats.StatsExpedition },
            DungeonMode.Mists => new DungeonStats { StatsMists = Stats.StatsMists },
            DungeonMode.MistsDungeon => new DungeonStats { StatsMistsDungeon = Stats.StatsMistsDungeon },
            DungeonMode.AbyssalDepths => new DungeonStats { StatsAbyssalDepths = Stats.StatsAbyssalDepths },
            _ => new DungeonStats { StatsTotal = Stats.StatsTotal }
        };
    }

    public static string TranslationTimeRange => LocalizationController.Translation("TIME_RANGE");
    public static string TranslationTier => LocalizationController.Translation("TIER");
    public static string TranslationEnchantment => LocalizationController.Translation("ENCHANTMENT");
    public static string TranslationContentRuns => LocalizationController.Translation("CONTENT_RUNS");
    public static string TranslationDeleteAndReset => LocalizationController.Translation("DELETE_AND_RESET");
    public static string TranslationSettings => LocalizationController.Translation("SETTINGS");
    public static string TranslationSettingsAndReset => LocalizationController.Translation("SETTINGS_AND_RESET");

    private sealed record FilteredDungeonPeriods(List<DungeonBaseFragment> Current, List<DungeonBaseFragment> Previous);
}
