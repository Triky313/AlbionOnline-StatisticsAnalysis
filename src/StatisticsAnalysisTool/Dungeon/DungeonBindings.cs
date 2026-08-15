using Serilog;
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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonBindings : BaseViewModel
{
    private const int RefreshDelayMilliseconds = 250;

    private readonly ObservableRangeCollection<DungeonBaseFragment> _filteredDungeons = [];
    private readonly HashSet<DungeonBaseFragment> _subscribedDungeons = [];
    private readonly Timer _scheduledRefreshTimer;
    private int _filterUpdateVersion;
    private int _hasScheduledRefresh;
    private DashboardChartRangeOption _selectedStatsTimeType;
    private StatsTypeFilterStruct _selectedDungeonStatsType;

    public DungeonBindings()
    {
        _scheduledRefreshTimer = new Timer(_ => StartScheduledRefresh(), null, Timeout.Infinite, Timeout.Infinite);
        DungeonsCollectionView = new ListCollectionView(_filteredDungeons);
        Dungeons.CollectionChanged += OnDungeonsCollectionChanged;
        DungeonStatsFilter = new DungeonStatsFilter(this);
        DungeonOptionsObject.PlayerLootVisibilityChanged += OnPlayerLootVisibilityChanged;
        RefreshLocalization();
    }

    public ObservableRangeCollection<DungeonBaseFragment> Dungeons { get; } = new();

    public ListCollectionView DungeonsCollectionView { get; }

    public DungeonStats Stats
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public DungeonAnalytics Analytics { get; } = new();

    public DungeonStatsFilter DungeonStatsFilter
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public DungeonOptionsObject DungeonOptionsObject
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public GridLength GridSplitterPosition
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.DungeonsGridSplitterPosition = field.Value;
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
        get;
        set
        {
            field = value;
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
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

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
        DungeonStatsFilter.RefreshLocalization();
        foreach (var dungeon in Dungeons)
        {
            dungeon.RefreshLootVisibility();
            dungeon.RefreshPerformanceMetrics();
        }

        OnPropertyChanged(null);
        _ = UpdateFilteredDungeonsAsync();
    }

    public void RequestUpdateFilteredDungeons()
    {
        Interlocked.Increment(ref _filterUpdateVersion);
        Interlocked.Exchange(ref _hasScheduledRefresh, 1);
        _scheduledRefreshTimer.Change(RefreshDelayMilliseconds, Timeout.Infinite);
    }

    private void StartScheduledRefresh()
    {
        if (Interlocked.Exchange(ref _hasScheduledRefresh, 0) == 0)
        {
            return;
        }

        _ = UpdateFilteredDungeonsAfterDelayAsync();
    }

    private async Task UpdateFilteredDungeonsAfterDelayAsync()
    {
        try
        {
            await UpdateFilteredDungeonsCoreAsync();
        }
        catch (Exception exception)
        {
            Log.Error(exception, "Failed to refresh dungeon data.");
        }
    }

    private void CancelScheduledRefresh()
    {
        Interlocked.Exchange(ref _hasScheduledRefresh, 0);
        _scheduledRefreshTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    public async Task UpdateFilteredDungeonsAsync()
    {
        CancelScheduledRefresh();
        await UpdateFilteredDungeonsCoreAsync();
    }

    private async Task UpdateFilteredDungeonsCoreAsync()
    {
        var filterUpdateVersion = Interlocked.Increment(ref _filterUpdateVersion);
        var request = await CreateFilterRequestAsync();

        if (request.SelectedRange == null || IsFilterUpdateObsolete(filterUpdateVersion))
        {
            return;
        }

        var rangeDuration = GetDuration(request.SelectedRange);
        var currentRangeEnd = DateTime.UtcNow;
        var currentRangeStart = currentRangeEnd.Subtract(rangeDuration);
        var previousRangeStart = currentRangeStart.Subtract(rangeDuration);
        var appliesTierFilter = SupportsTier(request.SelectedMode);
        var appliesEnchantmentFilter = SupportsEnchantment(request.SelectedMode);

        var filteredPeriods = await Task.Run(() =>
        {
            var current = new List<DungeonBaseFragment>();
            var previous = new List<DungeonBaseFragment>();

            foreach (var dungeon in request.Dungeons)
            {
                if (IsFilterUpdateObsolete(filterUpdateVersion))
                {
                    return null;
                }

                if (!MatchesMetadata(
                        dungeon,
                        request.SelectedMode,
                        request.SelectedTiers,
                        request.SelectedLevels,
                        appliesTierFilter,
                        appliesEnchantmentFilter))
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
        });

        if (filteredPeriods == null || IsFilterUpdateObsolete(filterUpdateVersion))
        {
            return;
        }

        await ApplyFilteredPeriodsAsync(
            filteredPeriods,
            request.SelectedRange,
            request.SelectedMode,
            filterUpdateVersion);
    }

    private async Task<DungeonFilterRequest> CreateFilterRequestAsync()
    {
        DungeonFilterRequest request = null;

        void CreateRequest()
        {
            request = new DungeonFilterRequest(
                Dungeons.ToList(),
                SelectedDungeonStatsType.StatsViewType,
                DungeonStatsFilter.TierFilters.ToHashSet(),
                DungeonStatsFilter.LevelFilters.ToHashSet(),
                SelectedStatsTimeType ?? DungeonStatTimeTypes.FirstOrDefault());
        }

        if (Application.Current?.Dispatcher is { } dispatcher && !dispatcher.CheckAccess())
        {
            await dispatcher.InvokeAsync(CreateRequest);
        }
        else
        {
            CreateRequest();
        }

        return request;
    }

    private void OnDungeonsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        SynchronizeDungeonSubscriptions();
        RequestUpdateFilteredDungeons();
    }

    private void SynchronizeDungeonSubscriptions()
    {
        var currentDungeons = Dungeons.ToHashSet();

        foreach (var removedDungeon in _subscribedDungeons.Where(x => !currentDungeons.Contains(x)).ToList())
        {
            removedDungeon.PropertyChanged -= OnDungeonPropertyChanged;
            _subscribedDungeons.Remove(removedDungeon);
        }

        foreach (var addedDungeon in currentDungeons.Where(_subscribedDungeons.Add))
        {
            addedDungeon.PropertyChanged += OnDungeonPropertyChanged;
        }
    }

    private void OnDungeonPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not DungeonBaseFragment dungeon
            || dungeon.Status != DungeonStatus.Active && e.PropertyName != nameof(DungeonBaseFragment.Status))
        {
            return;
        }

        RequestUpdateFilteredDungeons();
    }

    private async Task ApplyFilteredPeriodsAsync(
        FilteredDungeonPeriods filteredPeriods,
        DashboardChartRangeOption selectedRange,
        DungeonMode selectedMode,
        int filterUpdateVersion)
    {
        void Apply()
        {
            if (IsFilterUpdateObsolete(filterUpdateVersion))
            {
                return;
            }

            if (!_filteredDungeons.SequenceEqual(filteredPeriods.Current))
            {
                _filteredDungeons.ReplaceRange(filteredPeriods.Current);
            }

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

    private bool IsFilterUpdateObsolete(int filterUpdateVersion)
    {
        return Volatile.Read(ref _filterUpdateVersion) != filterUpdateVersion;
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
            or DungeonMode.AbyssalDepths
            or DungeonMode.StaticDungeon;
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
            CreateContentTab("STATIC_DUNGEONS", DungeonMode.StaticDungeon),
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
            DungeonMode.StaticDungeon => new DungeonStats { StatsTotal = Stats.StatsTotal },
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

    private sealed record DungeonFilterRequest(
        List<DungeonBaseFragment> Dungeons,
        DungeonMode SelectedMode,
        IReadOnlySet<Tier> SelectedTiers,
        IReadOnlySet<ItemLevel> SelectedLevels,
        DashboardChartRangeOption SelectedRange);

    private sealed record FilteredDungeonPeriods(List<DungeonBaseFragment> Current, List<DungeonBaseFragment> Previous);
}
