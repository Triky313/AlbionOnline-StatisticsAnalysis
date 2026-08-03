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
    private readonly Dispatcher _uiDispatcher;
    private readonly DispatcherTimer _dashboardChartRefreshTimer;
    private readonly HashSet<ValueType> _chartValueTypes =
    [
        ValueType.Fame,
        ValueType.Silver,
        ValueType.ReSpec,
        ValueType.FactionFame,
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

    public void StartSession(string characterName)
    {
        if (string.IsNullOrWhiteSpace(characterName) || !AppDataPaths.IsUserDataAvailable)
        {
            Log.Warning(
                "Statistics session was not started because login metadata is incomplete. Character={Character}, Server={Server}",
                characterName,
                AppDataPaths.ActiveUserDataServerLocation);
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
            Log.Information(
                "Statistics session started. Character={Character}, Server={Server}",
                characterName,
                AppDataPaths.ActiveUserDataServerLocation);
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
            .Select(x => AddBuckets(x, -selectedRange.BucketCount, selectedRange.Unit));
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
        UpdateDashboardContentRankings(selectedRange, currentRangeBucketStarts);

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
