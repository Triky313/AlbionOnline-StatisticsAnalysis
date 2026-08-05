using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Serilog;
using SkiaSharp;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.BindingModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Manager;

public class StatisticController
{
    private static readonly TimeSpan DashboardChartRefreshDelay = TimeSpan.FromMilliseconds(500);
    private const int DashboardContentRankingLimit = 8;

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly object _syncRoot = new();
    private readonly StatisticSessionStorage _sessionStorage = new();
    private readonly Dictionary<Guid, long> _dirtySessionVersions = [];
    private readonly SemaphoreSlim _sessionPersistenceSemaphore = new(1, 1);
    private readonly Dispatcher _uiDispatcher;
    private readonly DispatcherTimer _dashboardChartRefreshTimer;
    private readonly HashSet<ValueType> _chartValueTypes =
    [
        ValueType.Fame,
        ValueType.Silver,
        ValueType.ReSpec,
        ValueType.PaidSilverForReSpec,
        ValueType.RepairCosts,
        ValueType.FactionStanding,
        ValueType.FactionPoints,
        ValueType.Might,
        ValueType.Favor
    ];

    private int _isDashboardChartRefreshSchedulingPending;
    private DashboardStatistics _dashboardStatistics = new();
    private DashboardStatisticsAggregator _statisticsAggregator = new(new DashboardStatistics());

    public StatisticController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
        _uiDispatcher = Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher;
        _dashboardChartRefreshTimer = new DispatcherTimer(
            DashboardChartRefreshDelay,
            DispatcherPriority.Background,
            OnDashboardChartRefreshTimerTick,
            _uiDispatcher);
        _dashboardChartRefreshTimer.Stop();
    }

    #region Dashboard

    public bool HasActiveSession
    {
        get
        {
            lock (_syncRoot)
            {
                return _dashboardStatistics.GetActiveSession() != null;
            }
        }
    }

    public void AddValue(ValueType valueType, double gainedValue, CityFaction cityFaction = CityFaction.Unknown)
    {
        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug(
                    "Statistics value discarded because no active session exists. ValueType={ValueType}",
                    valueType);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = valueType,
                Value = gainedValue,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                CityFaction = cityFaction
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkSessionDirtyInternal(session.Id);
        }

        if (_chartValueTypes.Contains(valueType))
        {
            UpdateDailyChart();
        }

        if (valueType == ValueType.RepairCosts)
        {
            UpdateRepairCostsUi();
        }
    }

    public void AddLootValue(int itemIndex, int quantity, double unitValue)
    {
        if (itemIndex <= 0
            || quantity <= 0
            || !double.IsFinite(unitValue))
        {
            return;
        }

        if (!_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);
        var lootAreaIndex = ResolveLootAreaIndex(mapType, dungeonMode);
        var lootAreaClusterType = string.IsNullOrWhiteSpace(lootAreaIndex)
            ? ClusterType.Unknown
            : WorldData.GetClusterTypeByIndex(lootAreaIndex);
        var totalValue = Math.Max(0, unitValue) * quantity;

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug("Statistics value discarded because no active session exists. ValueType={ValueType}", ValueType.LootValue);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = ValueType.LootValue,
                Value = totalValue,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                CityFaction = CityFaction.Unknown,
                ItemIndex = itemIndex,
                ItemQuantity = quantity,
                LootAreaIndex = lootAreaIndex,
                LootAreaClusterType = lootAreaClusterType
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    public void AddLootedChest(TreasureRarity treasureRarity)
    {
        if (treasureRarity == TreasureRarity.Unknown
            || !_trackingController.IsTrackingAllowedByMainCharacter())
        {
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var mapType = ClusterController.CurrentCluster.MapType;
        var dungeonMode = ResolveDungeonMode(mapType);
        var lootAreaIndex = ResolveLootAreaIndex(mapType, dungeonMode);
        var lootAreaClusterType = string.IsNullOrWhiteSpace(lootAreaIndex)
            ? ClusterType.Unknown
            : WorldData.GetClusterTypeByIndex(lootAreaIndex);

        lock (_syncRoot)
        {
            var session = _dashboardStatistics.GetActiveSession();
            if (session == null)
            {
                Log.Debug("Statistics value discarded because no active session exists. ValueType={ValueType}", ValueType.LootedChest);
                return;
            }

            var statisticEntry = new StatisticEntry
            {
                SessionId = session.Id,
                OccurredAtUtc = nowUtc,
                ValueType = ValueType.LootedChest,
                Value = 1,
                MapType = mapType,
                DungeonMode = dungeonMode,
                ClusterMode = ClusterController.CurrentCluster.ClusterMode,
                CityFaction = CityFaction.Unknown,
                LootAreaIndex = lootAreaIndex,
                LootAreaClusterType = lootAreaClusterType,
                TreasureRarity = treasureRarity
            };

            _dashboardStatistics.Add(statisticEntry);
            _statisticsAggregator.Add(statisticEntry);
            MarkSessionDirtyInternal(session.Id);
        }

        UpdateDailyChart();
    }

    public void StartSession(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName) || !AppDataPaths.IsUserDataAvailable)
        {
            Log.Warning("Statistics session was not started because login metadata is incomplete. Character={Character}, Server={Server}", characterName, AppDataPaths.ActiveUserDataServerLocation);
            return;
        }

        var nowUtc = DateTime.UtcNow;
        var isNewSession = false;

        lock (_syncRoot)
        {
            var previousSessionId = _dashboardStatistics.GetActiveSession()?.Id;
            var session = _dashboardStatistics.StartSession(
                characterName,
                AppDataPaths.ActiveUserDataServerLocation,
                nowUtc);
            isNewSession = previousSessionId != session.Id;

            if (isNewSession)
            {
                MarkSessionDirtyInternal(session.Id);
            }
        }

        if (isNewSession)
        {
            _trackingController.LiveStatsTracker?.Reset();
            _trackingController.LiveStatsTracker?.Start();
            Log.Information("Statistics session started. Character={Character}, Server={Server}", characterName, AppDataPaths.ActiveUserDataServerLocation);
        }

        RefreshDashboardSessionFilters();
    }

    public bool EndSession(DateTime endedAtUtc)
    {
        bool wasEnded;
        lock (_syncRoot)
        {
            var activeSessionId = _dashboardStatistics.GetActiveSession()?.Id;
            wasEnded = _dashboardStatistics.EndActiveSession(endedAtUtc);

            if (wasEnded && activeSessionId.HasValue)
            {
                MarkSessionDirtyInternal(activeSessionId.Value);
            }
        }

        if (!wasEnded)
        {
            return false;
        }

        _trackingController.LiveStatsTracker?.Stop();
        RefreshDashboardSessionFilters();
        UpdateDailyChart(true);
        Log.Information("Statistics session ended");
        return true;
    }

    public async System.Threading.Tasks.Task<bool> ResetSessionAsync()
    {
        string characterName;
        lock (_syncRoot)
        {
            var activeSession = _dashboardStatistics.GetActiveSession();
            if (activeSession == null)
            {
                return false;
            }

            characterName = activeSession.CharacterName;
        }

        if (!EndSession(DateTime.UtcNow))
        {
            return false;
        }

        await SaveInFileAsync();
        StartSession(characterName);
        Log.Information("Statistics session reset");
        return true;
    }

    public async System.Threading.Tasks.Task<bool> DeleteSessionAsync(Guid sessionId)
    {
        if (sessionId == Guid.Empty)
        {
            return false;
        }

        StatisticSession session;
        lock (_syncRoot)
        {
            session = _dashboardStatistics
                .CreateSessionSnapshot()
                .FirstOrDefault(x => x.Id == sessionId);
        }

        if (session == null)
        {
            return false;
        }

        var wasActive = false;
        await _sessionPersistenceSemaphore.WaitAsync();

        try
        {
            if (!_sessionStorage.DeleteSession(sessionId))
            {
                return false;
            }

            lock (_syncRoot)
            {
                wasActive = _dashboardStatistics.GetActiveSession()?.Id == sessionId;
                _dashboardStatistics.RemoveSession(sessionId);
                _dirtySessionVersions.Remove(sessionId);
            }
        }
        finally
        {
            _sessionPersistenceSemaphore.Release();
        }

        if (wasActive)
        {
            _trackingController.LiveStatsTracker?.Stop();
        }

        if (wasActive && _mainWindowViewModel.MainStatusBindings.IsInGame)
        {
            StartSession(session.CharacterName);
        }
        else
        {
            RefreshDashboardSessionFilters();
        }

        UpdateRepairCostsUi();
        UpdateDailyChart(true);
        Log.Information(
            "Statistics session deleted. SessionId={SessionId}, WasActive={WasActive}",
            sessionId,
            wasActive);
        return true;
    }

    public void UpdateDailyChart(bool forceUpdate = false)
    {
        if (!forceUpdate)
        {
            ScheduleDashboardChartRefresh();
            return;
        }

        if (!_uiDispatcher.CheckAccess())
        {
            _ = _uiDispatcher.InvokeAsync(
                () => UpdateDailyChart(true),
                DispatcherPriority.Background);
            return;
        }

        _dashboardChartRefreshTimer.Stop();

        var selectedRange = _mainWindowViewModel.SelectedDashboardChartRange;
        if (selectedRange == null)
        {
            return;
        }

        var selectedSeriesFilters = (_mainWindowViewModel.DashboardChartSeriesFilters ?? [])
            .Where(x => x.IsSelected)
            .ToList();

        var chartBuckets = CreateChartBuckets(selectedRange);
        var currentRangeBucketStarts = chartBuckets.Select(x => x.Start).ToArray();
        var previousRangeBucketStarts = currentRangeBucketStarts
            .Select(x => AddBuckets(x, -selectedRange.BucketCount, selectedRange.Unit))
            .ToArray();
        var aggregationBucketStarts = currentRangeBucketStarts
            .Concat(previousRangeBucketStarts).Distinct().ToArray();

        var xAxes = new[]
        {
            new Axis
            {
                LabelsRotation = 15,
                Labels = chartBuckets.Select(x => x.Label).ToArray()
            }
        };

        var aggregatedValues = _statisticsAggregator.AggregateChartValues(
            aggregationBucketStarts,
            selectedRange.Unit,
            _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId,
            _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType);
        UpdateDashboardSummary(selectedRange, chartBuckets, aggregatedValues);
        UpdateDashboardLootStatistics(
            selectedRange,
            currentRangeBucketStarts,
            previousRangeBucketStarts,
            aggregatedValues);
        UpdateDashboardLootedChestStatistics(
            selectedRange,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardContentRankings(selectedRange, currentRangeBucketStarts);
        UpdateDashboardEconomyStatistics(
            selectedRange,
            currentRangeBucketStarts,
            previousRangeBucketStarts,
            chartBuckets[0].Start,
            DateTime.UtcNow);

        if (selectedSeriesFilters.Count == 0)
        {
            _mainWindowViewModel.XAxesDashboardHourValues = xAxes;
            _mainWindowViewModel.SeriesDashboardHourValues = [];
            return;
        }

        var seriesCollection = new ObservableCollection<ISeries>();

        foreach (var selectedSeriesFilter in selectedSeriesFilters)
        {
            var valuesLookup = aggregatedValues.GetValueOrDefault(selectedSeriesFilter.ValueType) ?? [];
            var points = new ObservableCollection<ObservablePoint>();

            for (var i = 0; i < chartBuckets.Count; i++)
            {
                var value = valuesLookup.GetValueOrDefault(chartBuckets[i].Start);
                points.Add(new ObservablePoint(i, value));
            }

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = selectedSeriesFilter.Name,
                Values = points,
                Fill = GetValueTypeBrush(selectedSeriesFilter.ValueType, true),
                Stroke = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                GeometryStroke = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                GeometryFill = GetValueTypeBrush(selectedSeriesFilter.ValueType, false),
                GeometrySize = 5,
                YToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
            };

            seriesCollection.Add(lineSeries);
        }

        _mainWindowViewModel.XAxesDashboardHourValues = xAxes;
        _mainWindowViewModel.SeriesDashboardHourValues = seriesCollection;
    }

    private void ScheduleDashboardChartRefresh()
    {
        if (_uiDispatcher.CheckAccess())
        {
            StartDashboardChartRefreshTimer();
            return;
        }

        if (Interlocked.Exchange(ref _isDashboardChartRefreshSchedulingPending, 1) == 1)
        {
            return;
        }

        _ = _uiDispatcher.InvokeAsync(() =>
        {
            Interlocked.Exchange(ref _isDashboardChartRefreshSchedulingPending, 0);
            StartDashboardChartRefreshTimer();
        }, DispatcherPriority.Background);
    }

    private void StartDashboardChartRefreshTimer()
    {
        if (!_dashboardChartRefreshTimer.IsEnabled)
        {
            _dashboardChartRefreshTimer.Start();
        }
    }

    private void OnDashboardChartRefreshTimerTick(object sender, EventArgs e)
    {
        _dashboardChartRefreshTimer.Stop();
        UpdateDailyChart(true);
    }

    public void UpdateDashboardSessionTime(DateTime nowUtc)
    {
        var selectedRange = _mainWindowViewModel.SelectedDashboardChartRange;
        if (selectedRange == null)
        {
            return;
        }

        var currentPeriodStart = AlignToBucketStart(nowUtc.ToLocalTime(), selectedRange.Unit);
        var rangeStart = AddBuckets(currentPeriodStart, -(selectedRange.BucketCount - 1), selectedRange.Unit);

        UpdateDashboardSessionTime(selectedRange, rangeStart, nowUtc);
    }

    private void UpdateDashboardSummary(
        DashboardChartRangeOption selectedRange,
        IReadOnlyList<ChartBucket> chartBuckets,
        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues)
    {
        var currentRangeBucketStarts = chartBuckets.Select(x => x.Start).ToHashSet();
        var previousRangeBucketStarts = currentRangeBucketStarts
            .Select(x => AddBuckets(x, -selectedRange.BucketCount, selectedRange.Unit))
            .ToHashSet();

        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FameSummary,
            aggregatedValues,
            ValueType.Fame,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.ReSpecSummary,
            aggregatedValues,
            ValueType.ReSpec,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.SilverSummary,
            aggregatedValues,
            ValueType.Silver,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.MightSummary,
            aggregatedValues,
            ValueType.Might,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FavorSummary,
            aggregatedValues,
            ValueType.Favor,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardFactionSummary(
            selectedRange,
            currentRangeBucketStarts,
            previousRangeBucketStarts);

        _mainWindowViewModel.DashboardBindings.SummaryComparisonText = selectedRange.Unit switch
        {
            DashboardChartRangeUnit.Minute => DashboardBindings.TranslationVsPreviousMinutes,
            DashboardChartRangeUnit.Hour when selectedRange.BucketCount == 1 => DashboardBindings.TranslationVsPreviousHour,
            DashboardChartRangeUnit.Hour => DashboardBindings.TranslationVsPreviousHours,
            DashboardChartRangeUnit.Day when selectedRange.BucketCount == 1 => DashboardBindings.TranslationVsPreviousDay,
            DashboardChartRangeUnit.Day => DashboardBindings.TranslationVsPreviousDays,
            _ => DashboardBindings.TranslationVsPreviousDay
        };

        UpdateDashboardSessionTime(selectedRange, chartBuckets[0].Start, DateTime.UtcNow);
    }

    private void UpdateDashboardFactionSummary(
        DashboardChartRangeOption selectedRange,
        IReadOnlySet<DateTime> currentRangeBucketStarts,
        IReadOnlySet<DateTime> previousRangeBucketStarts)
    {
        var factionValues = _statisticsAggregator.AggregateChartValues(
            currentRangeBucketStarts.Concat(previousRangeBucketStarts).ToArray(),
            selectedRange.Unit,
            _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId,
            _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType,
            _mainWindowViewModel.DashboardBindings.SelectedFactionOption.Faction);

        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FactionPointsSummary,
            factionValues,
            ValueType.FactionPoints,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
        UpdateDashboardSummaryMetric(
            _mainWindowViewModel.DashboardBindings.FactionStandingSummary,
            factionValues,
            ValueType.FactionStanding,
            currentRangeBucketStarts,
            previousRangeBucketStarts);
    }

    private void UpdateDashboardEconomyStatistics(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts,
        IReadOnlyCollection<DateTime> previousRangeBucketStarts,
        DateTime currentRangeStart,
        DateTime nowUtc)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var currentValues = _statisticsAggregator.AggregateEconomyValues(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId);
        var previousValues = _statisticsAggregator.AggregateEconomyValues(
            previousRangeBucketStarts,
            selectedRange.Unit,
            sessionId);
        var sessionCount = CountFilteredSessions(currentRangeStart, nowUtc, sessionId);
        var bindings = _mainWindowViewModel.DashboardBindings;

        bindings.EconomyReSpecSummary.Update(
            currentValues.ReSpec,
            currentValues.ReSpec,
            previousValues.ReSpec);
        bindings.RepairCostsSummary.Update(
            currentValues.RepairCosts,
            currentValues.RepairCosts,
            previousValues.RepairCosts);
        bindings.ReSpecSilverCost = currentValues.ReSpecSilverCost;
        bindings.AverageReSpecSilverCostPerSession = sessionCount > 0
            ? currentValues.ReSpecSilverCost / sessionCount
            : 0;
        bindings.SpentReSpec = currentValues.SpentReSpec;
        bindings.SpentReSpecVisibility = currentValues.SpentReSpec > 0
            ? Visibility.Visible
            : Visibility.Collapsed;
        bindings.ReSpecDetailColumnCount = currentValues.SpentReSpec > 0
            ? 3
            : 2;
        bindings.AverageRepairCostPerSession = sessionCount > 0
            ? currentValues.RepairCosts / sessionCount
            : 0;
        bindings.HighestRepairCost = currentValues.HighestRepairCost;
    }

    private void UpdateDashboardLootStatistics(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts,
        IReadOnlyCollection<DateTime> previousRangeBucketStarts,
        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var contentType = _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType;
        var entries = _statisticsAggregator.GetLootEntries(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);
        var values = aggregatedValues.GetValueOrDefault(ValueType.LootValue) ?? [];
        var currentValue = values
            .Where(x => currentRangeBucketStarts.Contains(x.Key))
            .Sum(x => x.Value);
        var previousValue = values
            .Where(x => previousRangeBucketStarts.Contains(x.Key))
            .Sum(x => x.Value);
        var lootStatistics = _mainWindowViewModel.DashboardBindings.LootStatistics;

        lootStatistics.TotalValueSummary.Update(
            currentValue,
            currentValue,
            previousValue);
        lootStatistics.AverageValue = entries.Count > 0
            ? currentValue / entries.Count
            : 0;

        var lootItems = new List<DashboardLootItem>(entries.Count);
        foreach (var entry in entries)
        {
            var item = ItemController.GetItemByIndex(entry.ItemIndex);
            if (item == null)
            {
                continue;
            }

            lootItems.Add(new DashboardLootItem(
                item,
                entry.ItemQuantity,
                entry.Value,
                entry.OccurredAtUtc));
        }

        ReplaceDashboardItems(
            lootStatistics.RecentItems,
            lootItems
                .OrderByDescending(x => x.LootedAtLocal)
                .Take(10));
        ReplaceDashboardItems(
            lootStatistics.MostValuableItems,
            lootItems
                .OrderByDescending(x => x.UnitValue)
                .ThenByDescending(x => x.LootedAtLocal)
                .Take(10)
                .Select(x => new DashboardLootItem(
                    x.Item,
                    x.Quantity,
                    x.TotalValue,
                    x.LootedAtLocal,
                    displayUnitValue: true)));
        ReplaceDashboardItems(lootStatistics.TopAreas, CreateTopLootAreas(entries));
    }

    private void UpdateDashboardLootedChestStatistics(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts,
        IReadOnlyCollection<DateTime> previousRangeBucketStarts)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var contentType = _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType;
        var currentEntries = _statisticsAggregator.GetLootedChestEntries(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);
        var previousEntries = _statisticsAggregator.GetLootedChestEntries(
            previousRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            contentType);
        var currentEntriesByContent = currentEntries
            .GroupBy(ResolveLootedChestContentType)
            .ToDictionary(group => group.Key, group => group.ToArray());
        var previousTotalsByContent = previousEntries
            .GroupBy(ResolveLootedChestContentType)
            .ToDictionary(group => group.Key, group => group.Count());

        foreach (var contentStatistics in _mainWindowViewModel.DashboardBindings.LootedChests.ContentStatistics)
        {
            var contentEntries = currentEntriesByContent.GetValueOrDefault(contentStatistics.ContentType) ?? [];
            UpdateDashboardLootedChestContentStatistics(
                contentStatistics,
                contentEntries,
                previousTotalsByContent.GetValueOrDefault(contentStatistics.ContentType));
        }
    }

    private static void UpdateDashboardLootedChestContentStatistics(
        DashboardLootedChestContentStatistics contentStatistics,
        IReadOnlyCollection<StatisticEntry> currentEntries,
        int previousTotal)
    {
        var common = 0;
        var uncommon = 0;
        var rare = 0;
        var legendary = 0;

        foreach (var entry in currentEntries)
        {
            switch (entry.TreasureRarity)
            {
                case TreasureRarity.Common:
                    common++;
                    break;
                case TreasureRarity.Uncommon:
                    uncommon++;
                    break;
                case TreasureRarity.Rare:
                    rare++;
                    break;
                case TreasureRarity.Legendary:
                    legendary++;
                    break;
            }
        }

        var total = currentEntries.Count;
        var mapCount = currentEntries
            .Select(CreateLootedChestAreaKey)
            .Distinct(StringComparer.Ordinal)
            .Count();
        var averagePerMap = mapCount > 0
            ? (double) total / mapCount
            : 0;

        contentStatistics.Update(
            total,
            previousTotal,
            common,
            uncommon,
            rare,
            legendary,
            averagePerMap);
    }

    private static DashboardContentType ResolveLootedChestContentType(StatisticEntry entry)
    {
        return DashboardContentTypeResolver.Resolve(
            entry.MapType,
            entry.DungeonMode,
            entry.ClusterMode);
    }

    private static string CreateLootedChestAreaKey(StatisticEntry entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.LootAreaIndex))
        {
            return $"map:{entry.LootAreaIndex}";
        }

        return entry.DungeonMode != DungeonMode.Unknown
            ? $"content:{entry.DungeonMode}"
            : $"content:{entry.MapType}:{entry.ClusterMode}";
    }
    private static void ReplaceDashboardItems<T>(
        ObservableCollection<T> target,
        IEnumerable<T> items)
    {
        target.Clear();
        foreach (var item in items)
        {
            target.Add(item);
        }
    }

    private static IReadOnlyCollection<DashboardLootAreaItem> CreateTopLootAreas(IReadOnlyCollection<StatisticEntry> entries)
    {
        return entries
            .Where(HasKnownLootArea)
            .GroupBy(GetLootAreaKey)
            .Select(group => new
            {
                group.Key,
                ItemCount = group.Sum(entry => (long) entry.ItemQuantity),
                TotalValue = group.Sum(entry => entry.Value),
                ClusterType = group.First().LootAreaClusterType
            })
            .OrderByDescending(area => area.TotalValue)
            .ThenBy(area => area.Key.AreaIndex, StringComparer.OrdinalIgnoreCase)
            .ThenBy(area => area.Key.DungeonMode)
            .Take(5)
            .Select(area => new DashboardLootAreaItem(
                ResolveLootAreaName(area.Key.DungeonMode, area.Key.AreaIndex),
                area.ItemCount,
                area.TotalValue,
                area.ClusterType,
                area.Key.DungeonMode))
            .ToArray();
    }

    private static bool HasKnownLootArea(StatisticEntry entry)
    {
        return IsStandaloneLootArea(entry.DungeonMode)
               || !string.IsNullOrWhiteSpace(entry.LootAreaIndex);
    }

    private static (DungeonMode DungeonMode, string AreaIndex) GetLootAreaKey(StatisticEntry entry)
    {
        return IsStandaloneLootArea(entry.DungeonMode)
            ? (entry.DungeonMode, string.Empty)
            : (DungeonMode.Unknown, entry.LootAreaIndex ?? string.Empty);
    }

    private static string ResolveLootAreaName(DungeonMode dungeonMode, string areaIndex)
    {
        if (!IsStandaloneLootArea(dungeonMode))
        {
            var mapName = WorldData.GetUniqueNameOrDefault(areaIndex);
            return string.IsNullOrWhiteSpace(mapName) ? areaIndex : mapName;
        }

        var translationKey = dungeonMode switch
        {
            DungeonMode.HellGate => "HELLGATE",
            DungeonMode.Corrupted => "CORRUPTED",
            DungeonMode.Expedition => "EXPEDITION",
            DungeonMode.Mists => "MISTS",
            DungeonMode.MistsDungeon => "MISTS_DUNGEON",
            DungeonMode.AbyssalDepths => "ABYSSALDEPTHS",
            DungeonMode.DragonArea => "DRAGONAREA",
            _ => "UNKNOWN"
        };

        return LocalizationController.Translation(translationKey);
    }

    private int CountFilteredSessions(DateTime localRangeStart, DateTime nowUtc, Guid? selectedSessionId)
    {
        List<StatisticSession> sessions;
        lock (_syncRoot)
        {
            sessions = _dashboardStatistics.CreateSessionSnapshot();
        }

        var rangeStartUtc = localRangeStart.ToUniversalTime();
        return sessions.Count(session =>
            (!selectedSessionId.HasValue || session.Id == selectedSessionId.Value)
            && session.StartedAtUtc < nowUtc
            && (session.EndedAtUtc ?? nowUtc) > rangeStartUtc);
    }

    private void UpdateDashboardContentRankings(
        DashboardChartRangeOption selectedRange,
        IReadOnlyCollection<DateTime> currentRangeBucketStarts)
    {
        var sessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var contentType = _mainWindowViewModel.SelectedDashboardContentFilter?.ContentType;
        var fameValues = _statisticsAggregator.AggregateContentValues(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            ValueType.Fame,
            contentType);
        var silverValues = _statisticsAggregator.AggregateContentValues(
            currentRangeBucketStarts,
            selectedRange.Unit,
            sessionId,
            ValueType.Silver,
            contentType);

        UpdateDashboardContentRanking(
            _mainWindowViewModel.DashboardBindings.FameContentRanking,
            fameValues,
            value => _mainWindowViewModel.DashboardBindings.TotalFameByContent = value);
        UpdateDashboardContentRanking(
            _mainWindowViewModel.DashboardBindings.SilverContentRanking,
            silverValues,
            value => _mainWindowViewModel.DashboardBindings.TotalSilverByContent = value);
    }

    private static void UpdateDashboardContentRanking(
        ObservableCollection<DashboardContentRankingItem> ranking,
        IReadOnlyDictionary<(MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double> contentValues,
        Action<double> updateTotal)
    {
        var valuesByContent = contentValues
            .Where(x => x.Value > 0)
            .GroupBy(x => DashboardContentTypeResolver.Resolve(
                x.Key.MapType,
                x.Key.DungeonMode,
                x.Key.ClusterMode))
            .ToDictionary(x => x.Key, x => x.Sum(value => value.Value));
        var total = valuesByContent.Values.Sum();
        var topValues = valuesByContent
            .OrderByDescending(x => x.Value)
            .Take(DashboardContentRankingLimit)
            .ToList();
        var highestValue = topValues.FirstOrDefault().Value;

        ranking.Clear();
        updateTotal(total);

        foreach (var (contentType, value) in topValues)
        {
            var sharePercentage = total > 0 ? value / total * 100 : 0;
            var barPercentage = highestValue > 0 ? value / highestValue * 100 : 0;
            ranking.Add(new DashboardContentRankingItem(
                LocalizationController.Translation(DashboardContentTypeResolver.GetTranslationKey(contentType)),
                value,
                sharePercentage,
                barPercentage,
                ResolveContentBrush(contentType)));
        }
    }

    private static Brush ResolveContentBrush(DashboardContentType contentType)
    {
        var resourceKey = DashboardContentTypeResolver.GetBrushResourceKey(contentType);
        return Application.Current?.TryFindResource(resourceKey) as Brush ?? Brushes.Gray;
    }

    private static void UpdateDashboardSummaryMetric(
        DashboardSummaryMetric metric,
        IReadOnlyDictionary<ValueType, Dictionary<DateTime, double>> aggregatedValues,
        ValueType valueType,
        IReadOnlySet<DateTime> currentRangeBucketStarts,
        IReadOnlySet<DateTime> previousRangeBucketStarts)
    {
        var values = aggregatedValues.GetValueOrDefault(valueType) ?? [];
        var currentRangeValue = values.Where(x => currentRangeBucketStarts.Contains(x.Key)).Sum(x => x.Value);
        var previousRangeValue = values.Where(x => previousRangeBucketStarts.Contains(x.Key)).Sum(x => x.Value);

        metric.Update(currentRangeValue, currentRangeValue, previousRangeValue);
    }

    private void UpdateDashboardSessionTime(
        DashboardChartRangeOption selectedRange,
        DateTime rangeStart,
        DateTime nowUtc)
    {
        List<StatisticSession> sessions;
        lock (_syncRoot)
        {
            sessions = _dashboardStatistics.CreateSessionSnapshot();
        }

        var selectedSessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
        var selectedSessions = selectedSessionId.HasValue
            ? sessions.Where(x => x.Id == selectedSessionId.Value).ToArray()
            : sessions.ToArray();
        var previousRangeStart = AddBuckets(rangeStart, -selectedRange.BucketCount, selectedRange.Unit);

        var currentRangeSeconds = SumSessionDurationSeconds(selectedSessions, rangeStart.ToUniversalTime(), nowUtc, nowUtc);
        var previousRangeSeconds = SumSessionDurationSeconds(
            selectedSessions,
            previousRangeStart.ToUniversalTime(),
            rangeStart.ToUniversalTime(),
            nowUtc);

        _mainWindowViewModel.DashboardBindings.SessionTimeSummary.Update(
            currentRangeSeconds,
            currentRangeSeconds,
            previousRangeSeconds);
    }

    private static double SumSessionDurationSeconds(
        IEnumerable<StatisticSession> sessions,
        DateTime periodStartUtc,
        DateTime periodEndUtc,
        DateTime nowUtc)
    {
        return sessions.Sum(session =>
        {
            var sessionEndUtc = session.EndedAtUtc ?? nowUtc;
            var overlapStartUtc = session.StartedAtUtc > periodStartUtc ? session.StartedAtUtc : periodStartUtc;
            var overlapEndUtc = sessionEndUtc < periodEndUtc ? sessionEndUtc : periodEndUtc;
            return overlapEndUtc > overlapStartUtc
                ? (overlapEndUtc - overlapStartUtc).TotalSeconds
                : 0;
        });
    }

    private static DateTime AlignToBucketStart(DateTime localDateTime, DashboardChartRangeUnit unit)
    {
        return unit switch
        {
            DashboardChartRangeUnit.Minute => new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, localDateTime.Minute, 0),
            DashboardChartRangeUnit.Hour => new DateTime(localDateTime.Year, localDateTime.Month, localDateTime.Day, localDateTime.Hour, 0, 0),
            DashboardChartRangeUnit.Day => localDateTime.Date,
            _ => localDateTime.Date
        };
    }

    private static DateTime AddBuckets(DateTime bucketStart, int bucketCount, DashboardChartRangeUnit unit)
    {
        return unit switch
        {
            DashboardChartRangeUnit.Minute => bucketStart.AddMinutes(bucketCount),
            DashboardChartRangeUnit.Hour => bucketStart.AddHours(bucketCount),
            DashboardChartRangeUnit.Day => bucketStart.AddDays(bucketCount),
            _ => bucketStart.AddDays(bucketCount)
        };
    }

    private void MarkSessionDirtyInternal(Guid sessionId)
    {
        _dirtySessionVersions[sessionId] =
            _dirtySessionVersions.GetValueOrDefault(sessionId) + 1;
    }

    private DungeonMode ResolveDungeonMode(MapType mapType)
    {
        if (mapType != MapType.RandomDungeon)
        {
            return mapType switch
            {
                MapType.HellGate => DungeonMode.HellGate,
                MapType.CorruptedDungeon => DungeonMode.Corrupted,
                MapType.Expedition => DungeonMode.Expedition,
                MapType.Mists => DungeonMode.Mists,
                MapType.MistsDungeon => DungeonMode.MistsDungeon,
                MapType.AbyssalDepths => DungeonMode.AbyssalDepths,
                MapType.DragonArea => DungeonMode.DragonArea,
                _ => DungeonMode.Unknown
            };
        }

        var currentDungeonMode = _trackingController.DungeonController.GetCurrentDungeonMode();
        if (currentDungeonMode is DungeonMode.Solo or DungeonMode.Standard or DungeonMode.Avalon)
        {
            return currentDungeonMode;
        }

        var detectedDungeonMode = DungeonData.GetDungeonMode(
            ClusterController.CurrentCluster.SourceClusterIndex,
            ClusterController.CurrentCluster.Index,
            ClusterController.CurrentCluster.UniqueName,
            ClusterController.CurrentCluster.UniqueClusterName);

        return detectedDungeonMode is DungeonMode.Solo or DungeonMode.Standard or DungeonMode.Avalon
            ? detectedDungeonMode
            : DungeonMode.Unknown;
    }

    private static string ResolveLootAreaIndex(MapType mapType, DungeonMode dungeonMode)
    {
        if (IsStandaloneLootArea(dungeonMode))
        {
            return string.Empty;
        }

        var currentCluster = ClusterController.CurrentCluster;
        if (mapType == MapType.RandomDungeon
            && !string.IsNullOrWhiteSpace(currentCluster.SourceClusterIndex))
        {
            return currentCluster.SourceClusterIndex;
        }

        return currentCluster.Index ?? string.Empty;
    }

    private static bool IsStandaloneLootArea(DungeonMode dungeonMode)
    {
        return dungeonMode is DungeonMode.HellGate
            or DungeonMode.Corrupted
            or DungeonMode.Expedition
            or DungeonMode.Mists
            or DungeonMode.MistsDungeon
            or DungeonMode.AbyssalDepths
            or DungeonMode.DragonArea;
    }

    private static List<ChartBucket> CreateChartBuckets(DashboardChartRangeOption selectedRange)
    {
        var buckets = new List<ChartBucket>(selectedRange.BucketCount);
        var currentBucketStart = AlignToBucketStart(DateTime.Now, selectedRange.Unit);

        for (var i = selectedRange.BucketCount - 1; i >= 0; i--)
        {
            var start = AddBuckets(currentBucketStart, -i, selectedRange.Unit);
            var label = selectedRange.Unit switch
            {
                DashboardChartRangeUnit.Minute => start.ToString("HH:mm", CultureInfo.CurrentCulture),
                DashboardChartRangeUnit.Hour => start.ToString("dd.MM HH:mm", CultureInfo.CurrentCulture),
                DashboardChartRangeUnit.Day => start.ToString("d", CultureInfo.CurrentCulture),
                _ => start.ToString("d", CultureInfo.CurrentCulture)
            };

            buckets.Add(new ChartBucket(start, label));
        }

        return buckets;
    }

    public static SolidColorPaint GetValueTypeBrush(ValueType valueType, bool transparent)
    {
        try
        {
            if (transparent)
            {
                var transparentBrush = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}.Transparent"];
                return new SolidColorPaint
                {
                    Color = new SKColor(transparentBrush.Color.R, transparentBrush.Color.G, transparentBrush.Color.B, transparentBrush.Color.A)
                };
            }

            var brush = (SolidColorBrush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}"];
            return new SolidColorPaint
            {
                Color = new SKColor(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            };
        }
        catch
        {
            return new SolidColorPaint
            {
                Color = new SKColor(0, 0, 0, 0)
            };
        }
    }

    private sealed class ChartBucket
    {
        public ChartBucket(DateTime start, string label)
        {
            Start = start;
            Label = label;
        }

        public DateTime Start { get; }
        public string Label { get; }
    }

    #endregion

    #region Kill / Death infos

    public void SetKillsDeathsValues()
    {
        _mainWindowViewModel.DashboardBindings.KillsToday = _trackingController.EntityController.LocalUserData.KillsToday;
        _mainWindowViewModel.DashboardBindings.SoloKillsToday = _trackingController.EntityController.LocalUserData.SoloKillsToday;
        _mainWindowViewModel.DashboardBindings.DeathsToday = _trackingController.EntityController.LocalUserData.DeathsToday;
        _mainWindowViewModel.DashboardBindings.KillsThisWeek = _trackingController.EntityController.LocalUserData.KillsWeek;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisWeek = _trackingController.EntityController.LocalUserData.SoloKillsWeek;
        _mainWindowViewModel.DashboardBindings.DeathsThisWeek = _trackingController.EntityController.LocalUserData.DeathsWeek;
        _mainWindowViewModel.DashboardBindings.KillsThisMonth = _trackingController.EntityController.LocalUserData.KillsMonth;
        _mainWindowViewModel.DashboardBindings.SoloKillsThisMonth = _trackingController.EntityController.LocalUserData.SoloKillsMonth;
        _mainWindowViewModel.DashboardBindings.DeathsThisMonth = _trackingController.EntityController.LocalUserData.DeathsMonth;

        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenKilling = _trackingController.EntityController.LocalUserData.AverageItemPowerWhenKilling;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerOfTheKilledEnemies = _trackingController.EntityController.LocalUserData.AverageItemPowerOfTheKilledEnemies;
        _mainWindowViewModel.DashboardBindings.AverageItemPowerWhenDying = _trackingController.EntityController.LocalUserData.AverageItemPowerWhenDying;
        _mainWindowViewModel.DashboardBindings.LastUpdate = _trackingController.EntityController.LocalUserData.LastUpdate;
    }

    #endregion

    #region Repair costs stats

    public void UpdateRepairCostsUi()
    {
        var now = DateTime.Now;
        var endExclusive = now.AddTicks(1);
        var statisticsAggregator = _statisticsAggregator;

        _mainWindowViewModel.DashboardBindings.RepairCostsToday = FixPoint.FromFloatingPointValue(
            statisticsAggregator.SumRepairCosts(now.Date, endExclusive)).IntegerValue;
        _mainWindowViewModel.DashboardBindings.RepairCostsLast7Days = FixPoint.FromFloatingPointValue(
            statisticsAggregator.SumRepairCosts(now.AddDays(-7), endExclusive)).IntegerValue;
        _mainWindowViewModel.DashboardBindings.RepairCostsLast30Days = FixPoint.FromFloatingPointValue(
            statisticsAggregator.SumRepairCosts(now.AddDays(-30), endExclusive)).IntegerValue;
    }

    #endregion

    #region Load / Save local file data

    public async System.Threading.Tasks.Task LoadFromFileAsync()
    {
        var loadedStatistics = await _sessionStorage.LoadAsync(DateTime.UtcNow);

        lock (_syncRoot)
        {
            _dashboardStatistics = loadedStatistics;
            _statisticsAggregator = new DashboardStatisticsAggregator(loadedStatistics);
            _dirtySessionVersions.Clear();
        }

        if (_mainWindowViewModel.MainStatusBindings.IsInGame)
        {
            StartSession(_trackingController.EntityController.LocalUserData.Username ?? string.Empty);
        }
        else
        {
            RefreshDashboardSessionFilters();
        }

        UpdateRepairCostsUi();
        UpdateDailyChart(true);
    }

    public async System.Threading.Tasks.Task SaveInFileAsync()
    {
        await _sessionPersistenceSemaphore.WaitAsync();

        try
        {
            DashboardStatistics statisticsSnapshot;
            Dictionary<Guid, long> dirtySessionVersions;
            lock (_syncRoot)
            {
                statisticsSnapshot = _dashboardStatistics.CreateSnapshot();
                dirtySessionVersions = new Dictionary<Guid, long>(_dirtySessionVersions);
            }

            if (dirtySessionVersions.Count == 0)
            {
                return;
            }

            var wasSaved = await _sessionStorage.SaveSessionsAsync(
                statisticsSnapshot,
                dirtySessionVersions.Keys.ToArray());
            if (!wasSaved)
            {
                Log.Warning("Statistics session save was incomplete. Sessions={SessionCount}", dirtySessionVersions.Count);
                return;
            }

            lock (_syncRoot)
            {
                foreach (var savedSession in dirtySessionVersions)
                {
                    if (_dirtySessionVersions.TryGetValue(savedSession.Key, out var currentVersion)
                        && currentVersion == savedSession.Value)
                    {
                        _dirtySessionVersions.Remove(savedSession.Key);
                    }
                }
            }

            Log.Information("Statistics sessions saved. Sessions={SessionCount}", dirtySessionVersions.Count);
        }
        finally
        {
            _sessionPersistenceSemaphore.Release();
        }
    }

    private void RefreshDashboardSessionFilters()
    {
        IReadOnlyCollection<StatisticSession> sessionsSnapshot;
        lock (_syncRoot)
        {
            sessionsSnapshot = _dashboardStatistics.CreateSessionSnapshot();
        }

        void ApplyFilters()
        {
            var selectedSessionId = _mainWindowViewModel.SelectedDashboardSessionFilter?.SessionId;
            var filters = new List<DashboardSessionFilterOption>
            {
                new(null, LocalizationController.Translation("ALL_SESSIONS"))
            };

            filters.AddRange(sessionsSnapshot
                .OrderByDescending(x => x.StartedAtUtc)
                .Select(x => new DashboardSessionFilterOption(x.Id, CreateSessionFilterName(x))));

            _mainWindowViewModel.DashboardSessionFilters = new ObservableCollection<DashboardSessionFilterOption>(filters);
            _mainWindowViewModel.SelectedDashboardSessionFilter = filters
                .FirstOrDefault(x => x.SessionId == selectedSessionId)
                ?? filters[0];
        }

        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null || dispatcher.CheckAccess())
        {
            ApplyFilters();
            return;
        }

        _ = dispatcher.InvokeAsync(ApplyFilters);
    }

    private static string CreateSessionFilterName(StatisticSession session)
    {
        var activeMarker = session.EndedAtUtc.HasValue ? string.Empty : "* ";
        var characterName = string.IsNullOrWhiteSpace(session.CharacterName) ? "?" : session.CharacterName;
        return $"{activeMarker}{session.StartedAtUtc.ToLocalTime():g} | {characterName} | {session.ServerLocation}";
    }

    #endregion
}
