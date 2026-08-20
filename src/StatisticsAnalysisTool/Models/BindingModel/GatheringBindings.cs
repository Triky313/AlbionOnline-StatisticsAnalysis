using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Serilog;
using SkiaSharp;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class GatheringBindings : BaseViewModel
{
    private const int StatsUpdateDebounceMilliseconds = 75;
    private static readonly GatheringResourceType[] ResourceTypes =
    [
        GatheringResourceType.Wood,
        GatheringResourceType.Fiber,
        GatheringResourceType.Fishing,
        GatheringResourceType.Ore,
        GatheringResourceType.Hide,
        GatheringResourceType.Rock
    ];
    private readonly SemaphoreSlim _statsUpdateSemaphore = new(1, 1);
    private readonly object _pendingStatsSyncRoot = new();
    private readonly GatheringStatisticsCache _statisticsCache = new();
    private readonly HashSet<Gathered> _trackedGatheredEntries = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<Gathered> _pendingChangedEntries = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<Gathered> _pendingRemovedEntries = new(ReferenceEqualityComparer.Instance);
    private GatheringTimeRangeOption _selectedGatheringTimeRange;
    private GatheringSessionFilterOption _selectedGatheringSessionFilter;
    private bool _statisticsCacheInvalidated = true;
    private bool _overviewUpdateRequested = true;
    private bool _resourceChartUpdateRequested;
    private int _selectedGatheringTabIndex;
    private bool _isGatheringViewVisible;
    private int _statsUpdateVersion;

    public GatheringBindings()
    {
        GatheringTimeRanges = new ObservableCollection<GatheringTimeRangeOption>(GatheringTimeRangeOption.CreateDefault());
        _selectedGatheringTimeRange = GatheringTimeRanges.First(x => x is { BucketCount: 24, Unit: GatheringTimeRangeUnit.Hour });
        GatheringSessionFilters =
        [
            new GatheringSessionFilterOption(null, LocalizationController.Translation("ALL_SESSIONS"))
        ];
        _selectedGatheringSessionFilter = GatheringSessionFilters[0];
        IsGatheringActive = SettingsController.CurrentSettings.IsGatheringActive;

        foreach (var resourceChartSeriesFilter in GatheringStats.ResourceChartSeriesFilters)
        {
            resourceChartSeriesFilter.PropertyChanged += ResourceChartSeriesFilter_PropertyChanged;
        }

        GatheringStats.PropertyChanged += GatheringStats_PropertyChanged;

        GatheredCollection.CollectionChanged += GatheredCollection_CollectionChanged;
    }

    public void UpdateStats()
    {
        var updateVersion = Interlocked.Increment(ref _statsUpdateVersion);
        _ = UpdateStatsAsync(updateVersion);
    }

    private void GatheredCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            SynchronizeTrackedGatheredEntries();
            InvalidateStatisticsCache();
        }
        else
        {
            TrackAddedEntries(e.NewItems?.OfType<Gathered>() ?? []);
            TrackRemovedEntries(e.OldItems?.OfType<Gathered>() ?? []);
        }

        UpdateStats();
    }

    private void TrackAddedEntries(IEnumerable<Gathered> gatheredEntries)
    {
        lock (_pendingStatsSyncRoot)
        {
            foreach (var gathered in gatheredEntries)
            {
                if (_trackedGatheredEntries.Add(gathered))
                {
                    gathered.PropertyChanged += Gathered_PropertyChanged;
                }

                _pendingRemovedEntries.Remove(gathered);
                _pendingChangedEntries.Add(gathered);
            }
        }
    }

    private void TrackRemovedEntries(IEnumerable<Gathered> gatheredEntries)
    {
        lock (_pendingStatsSyncRoot)
        {
            foreach (var gathered in gatheredEntries)
            {
                if (_trackedGatheredEntries.Remove(gathered))
                {
                    gathered.PropertyChanged -= Gathered_PropertyChanged;
                }

                _pendingChangedEntries.Remove(gathered);
                _pendingRemovedEntries.Add(gathered);
            }
        }
    }

    private void SynchronizeTrackedGatheredEntries()
    {
        lock (_pendingStatsSyncRoot)
        {
            foreach (var gathered in _trackedGatheredEntries)
            {
                gathered.PropertyChanged -= Gathered_PropertyChanged;
            }

            _trackedGatheredEntries.Clear();
            _pendingChangedEntries.Clear();
            _pendingRemovedEntries.Clear();

            foreach (var gathered in GatheredCollection)
            {
                _trackedGatheredEntries.Add(gathered);
                gathered.PropertyChanged += Gathered_PropertyChanged;
            }
        }
    }

    private void Gathered_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (sender is not Gathered gathered || !IsStatisticsProperty(e.PropertyName))
        {
            return;
        }

        lock (_pendingStatsSyncRoot)
        {
            _pendingChangedEntries.Add(gathered);
        }

        UpdateStats();
    }

    private static bool IsStatisticsProperty(string propertyName)
    {
        return propertyName is nameof(Gathered.GainedStandardAmount)
            or nameof(Gathered.GainedBonusAmount)
            or nameof(Gathered.GainedPremiumBonusAmount)
            or nameof(Gathered.GainedFame)
            or nameof(Gathered.MiningProcesses)
            or nameof(Gathered.EstimatedMarketValue);
    }

    private void InvalidateStatisticsCache()
    {
        lock (_pendingStatsSyncRoot)
        {
            _statisticsCacheInvalidated = true;
        }
    }

    private async Task UpdateStatsAsync(int updateVersion)
    {
        await Task.Delay(StatsUpdateDebounceMilliseconds).ConfigureAwait(false);
        if (updateVersion != Volatile.Read(ref _statsUpdateVersion))
        {
            return;
        }

        await _statsUpdateSemaphore.WaitAsync().ConfigureAwait(false);
        try
        {
            if (updateVersion != Volatile.Read(ref _statsUpdateVersion))
            {
                return;
            }

            await UpdateStatsCoreAsync().ConfigureAwait(false);
        }
        finally
        {
            _statsUpdateSemaphore.Release();
        }
    }

    private async Task UpdateStatsCoreAsync()
    {
        try
        {
            var pendingUpdate = TakePendingStatsUpdate();
            var context = await Application.Current.Dispatcher.InvokeAsync(CreateStatsUpdateContext);
            var requiresRebuild = pendingUpdate.CacheInvalidated
                                  || !_statisticsCache.MatchesFilter(
                                      context.SessionId,
                                      context.TimeRange.BucketCount,
                                      context.TimeRange.Unit);
            var affectedResourceTypes = new HashSet<GatheringResourceType>();

            if (requiresRebuild)
            {
                var gatheredEntries = await Application.Current.Dispatcher.InvokeAsync(() => GatheredCollection.ToList());
                _statisticsCache.Rebuild(
                    gatheredEntries,
                    context.SessionId,
                    context.TimeRange.Start,
                    context.TimeRange.End,
                    context.TimeRange.BucketCount,
                    context.TimeRange.Unit);
                affectedResourceTypes.UnionWith(ResourceTypes);
            }
            else
            {
                foreach (var gathered in pendingUpdate.RemovedEntries)
                {
                    affectedResourceTypes.UnionWith(_statisticsCache.Remove(gathered));
                }

                foreach (var gathered in pendingUpdate.ChangedEntries)
                {
                    affectedResourceTypes.UnionWith(_statisticsCache.Update(gathered));
                }

                affectedResourceTypes.UnionWith(_statisticsCache.AdvanceTimeRange(
                    context.TimeRange.Start,
                    context.TimeRange.End));
            }

            var statisticsChanged = requiresRebuild || affectedResourceTypes.Count > 0;
            if (statisticsChanged)
            {
                await ApplyCoreStatisticsAsync(affectedResourceTypes).ConfigureAwait(false);
            }

            if (!context.IsOverviewVisible
                && (statisticsChanged
                    || pendingUpdate.OverviewUpdateRequested
                    || pendingUpdate.ResourceChartUpdateRequested))
            {
                MarkOverviewDirty();
            }

            if (context.IsOverviewVisible && (statisticsChanged || pendingUpdate.OverviewUpdateRequested))
            {
                await UpdateGatheringOverviewAsync().ConfigureAwait(false);
            }

            if (context.IsOverviewVisible
                && (pendingUpdate.ResourceChartUpdateRequested
                    || pendingUpdate.OverviewUpdateRequested
                    || ShouldUpdateResourceChart(affectedResourceTypes)))
            {
                UpdateResourceChart();
            }
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private PendingStatsUpdate TakePendingStatsUpdate()
    {
        lock (_pendingStatsSyncRoot)
        {
            var update = new PendingStatsUpdate(
                _pendingChangedEntries.ToList(),
                _pendingRemovedEntries.ToList(),
                _statisticsCacheInvalidated,
                _overviewUpdateRequested,
                _resourceChartUpdateRequested);
            _pendingChangedEntries.Clear();
            _pendingRemovedEntries.Clear();
            _statisticsCacheInvalidated = false;
            _overviewUpdateRequested = false;
            _resourceChartUpdateRequested = false;
            return update;
        }
    }

    private void MarkOverviewDirty()
    {
        lock (_pendingStatsSyncRoot)
        {
            _overviewUpdateRequested = true;
        }
    }

    private StatsUpdateContext CreateStatsUpdateContext()
    {
        return new StatsUpdateContext(
            GetTimeRange(SelectedGatheringTimeRange),
            SelectedGatheringSessionFilter?.SessionId,
            _isGatheringViewVisible && SelectedGatheringTabIndex == 0);
    }

    private async Task ApplyCoreStatisticsAsync(IReadOnlySet<GatheringResourceType> affectedResourceTypes)
    {
        var resourceUpdates = affectedResourceTypes
            .Where(ResourceTypes.Contains)
            .Select(resourceType => new ResourceStatsUpdate(
                resourceType,
                _statisticsCache.CreateGroupedResources(resourceType),
                _statisticsCache.GetResourceValue(resourceType),
                _statisticsCache.GetResourceValuePerHour(resourceType)))
            .ToList();
        var mostGatheredResource = _statisticsCache.CreateMostGatheredResource();
        var mostGatheredCluster = _statisticsCache.CreateMostGatheredCluster();

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var resourceUpdate in resourceUpdates)
            {
                ApplyResourceStats(resourceUpdate);
            }

            GatheringStats.MostGatheredResource = mostGatheredResource;
            GatheringStats.MostGatheredCluster = mostGatheredCluster;
            GatheringStats.TotalResources = _statisticsCache.TotalResources;
            GatheringStats.TotalMiningProcesses = _statisticsCache.TotalGatheringProcesses;
            GatheringStats.TotalGainedSilverString = _statisticsCache.TotalValue;
            GatheringStats.TotalGainedSilverPerHour = _statisticsCache.TotalValuePerHour;
        });
    }

    private void ApplyResourceStats(ResourceStatsUpdate update)
    {
        switch (update.ResourceType)
        {
            case GatheringResourceType.Hide:
                UpdateObservableRangeCollection(GatheringStats.GatheredHide, update.Resources);
                GatheringStats.GainedSilverByHide = update.TotalValue;
                GatheringStats.GainedSilverPerHourByHide = update.ValuePerHour;
                break;
            case GatheringResourceType.Ore:
                UpdateObservableRangeCollection(GatheringStats.GatheredOre, update.Resources);
                GatheringStats.GainedSilverByOre = update.TotalValue;
                GatheringStats.GainedSilverPerHourByOre = update.ValuePerHour;
                break;
            case GatheringResourceType.Fiber:
                UpdateObservableRangeCollection(GatheringStats.GatheredFiber, update.Resources);
                GatheringStats.GainedSilverByFiber = update.TotalValue;
                GatheringStats.GainedSilverPerHourByFiber = update.ValuePerHour;
                break;
            case GatheringResourceType.Wood:
                UpdateObservableRangeCollection(GatheringStats.GatheredWood, update.Resources);
                GatheringStats.GainedSilverByWood = update.TotalValue;
                GatheringStats.GainedSilverPerHourByWood = update.ValuePerHour;
                break;
            case GatheringResourceType.Rock:
                UpdateObservableRangeCollection(GatheringStats.GatheredRock, update.Resources);
                GatheringStats.GainedSilverByRock = update.TotalValue;
                GatheringStats.GainedSilverPerHourByRock = update.ValuePerHour;
                break;
            case GatheringResourceType.Fishing:
                UpdateObservableRangeCollection(GatheringStats.GatheredFish, update.Resources);
                GatheringStats.GainedSilverByFish = update.TotalValue;
                GatheringStats.GainedSilverPerHourByFish = update.ValuePerHour;
                break;
        }
    }

    private async Task UpdateGatheringOverviewAsync()
    {
        var totalValue = _statisticsCache.TotalValue;
        var totalAmount = _statisticsCache.TotalResources;
        var resourceSummaries = _statisticsCache.CreateResourceSummaries();
        var topGatheredResources = resourceSummaries
            .OrderByDescending(x => x.TotalAmount)
            .ThenByDescending(x => x.TotalValue)
            .Take(5)
            .Select((summary, index) => CreateRankedResourceSummary(summary, index + 1))
            .ToList();
        var mostGatheredResource = resourceSummaries
            .OrderByDescending(x => x.TotalAmount)
            .ThenByDescending(x => x.TotalValue)
            .FirstOrDefault();
        var bestResource = resourceSummaries
            .OrderByDescending(x => x.TotalValue)
            .ThenByDescending(x => x.TotalAmount)
            .FirstOrDefault();
        var mapSummaries = _statisticsCache.CreateMapSummaries();
        var bestMap = mapSummaries.FirstOrDefault();
        var resourceTypeSummaries = CreateResourceTypeSummaries(
            _statisticsCache.CreateResourceTypeTotals(),
            totalValue);
        var locationSummaries = CreateLocationSummaries(mapSummaries);
        var recentGatherings = _statisticsCache.GetRecentGatherings(5);
        var resourceValueByTypeSeries = CreatePieSeries(resourceTypeSummaries
            .Where(x => x.Value > 0)
            .Select(x => (x.Name, (double) x.Value, x.Brush)));
        var activityByLocationSeries = CreatePieSeries(locationSummaries
            .Where(x => x.TotalValue > 0)
            .Select(x => (x.Name, (double) x.TotalValue, x.Brush)));

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            GatheringStats.UniqueResourceTypes = resourceSummaries.Count;
            GatheringStats.AverageResourceValue = totalAmount > 0 ? (double) totalValue / totalAmount : 0;
            GatheringStats.BestSingleGatheringValue = _statisticsCache.BestSingleGatheringValue;
            GatheringStats.GatheringDurationSeconds = _statisticsCache.GatheringDurationSeconds;
            GatheringStats.BestResource = bestResource;
            GatheringStats.MostGatheredResourceDetails = mostGatheredResource;
            GatheringStats.BestGatheringMap = bestMap;
            GatheringStats.ResourceTypeSummaries.ReplaceRange(resourceTypeSummaries);
            GatheringStats.LocationSummaries.ReplaceRange(locationSummaries);
            GatheringStats.TopGatheredResources.ReplaceRange(topGatheredResources);
            GatheringStats.RecentGatherings.ReplaceRange(recentGatherings);
            GatheringStats.ResourceValueByTypeSeries = resourceValueByTypeSeries;
            GatheringStats.GatheringActivityByLocationSeries = activityByLocationSeries;
        });
    }

    private bool ShouldUpdateResourceChart(IReadOnlySet<GatheringResourceType> affectedResourceTypes)
    {
        return GatheringStats.ResourceChartSeriesFilters
            .Any(x => x.IsSelected && affectedResourceTypes.Contains(x.ResourceType));
    }

    private readonly record struct PendingStatsUpdate(
        IReadOnlyList<Gathered> ChangedEntries,
        IReadOnlyList<Gathered> RemovedEntries,
        bool CacheInvalidated,
        bool OverviewUpdateRequested,
        bool ResourceChartUpdateRequested);

    private readonly record struct StatsUpdateContext(
        GatheringTimeRange TimeRange,
        Guid? SessionId,
        bool IsOverviewVisible);

    private readonly record struct ResourceStatsUpdate(
        GatheringResourceType ResourceType,
        IReadOnlyList<Gathered> Resources,
        long TotalValue,
        double ValuePerHour);

    private static GatheringResourceSummary CreateRankedResourceSummary(GatheringResourceSummary summary, int rank)
    {
        return new GatheringResourceSummary
        {
            Rank = rank,
            UniqueName = summary.UniqueName,
            Item = summary.Item,
            TimesGathered = summary.TimesGathered,
            TotalAmount = summary.TotalAmount,
            TotalValue = summary.TotalValue,
            AverageValuePerGather = summary.AverageValuePerGather,
            GatheringDurationSeconds = summary.GatheringDurationSeconds,
            TopLocation = summary.TopLocation
        };
    }

    private static List<GatheringResourceTypeSummary> CreateResourceTypeSummaries(
        IReadOnlyCollection<GatheringStatisticsCache.ResourceTypeTotals> resourceTypeTotals,
        long totalValue)
    {
        var totalsByResourceType = resourceTypeTotals.ToDictionary(x => x.ResourceType);

        return ResourceTypes
            .Where(totalsByResourceType.ContainsKey)
            .Select(resourceType =>
            {
                var totals = totalsByResourceType[resourceType];
                return new GatheringResourceTypeSummary
                {
                    ResourceType = resourceType,
                    Name = GetResourceTypeName(resourceType),
                    Amount = totals.TotalAmount,
                    Value = totals.TotalValue,
                    SharePercentage = totalValue > 0 ? (double) totals.TotalValue / totalValue * 100 : 0,
                    Brush = GatheringChartSeriesFilter.GetBrush(resourceType)
                };
            })
            .OrderByDescending(x => x.Value)
            .ToList();
    }

    private static string GetResourceTypeName(GatheringResourceType resourceType)
    {
        return LocalizationController.Translation(resourceType.ToString().ToUpperInvariant());
    }

    private static List<GatheringMapSummary> CreateLocationSummaries(IReadOnlyList<GatheringMapSummary> mapSummaries)
    {
        var result = mapSummaries
            .Take(5)
            .Select((summary, index) => CopyMapSummary(summary, GetLocationBrush(index)))
            .ToList();
        var remainingMaps = mapSummaries.Skip(5).ToList();

        if (remainingMaps.Count > 0)
        {
            result.Add(new GatheringMapSummary
            {
                Name = LocalizationController.Translation("OTHER_MAPS"),
                TimesGathered = remainingMaps.Sum(x => x.TimesGathered),
                TotalValue = remainingMaps.Sum(x => x.TotalValue),
                ResourceTypeCount = remainingMaps.Sum(x => x.ResourceTypeCount),
                GatheringDurationSeconds = remainingMaps.Sum(x => x.GatheringDurationSeconds),
                Brush = GetLocationBrush(5)
            });
        }

        return result;
    }

    private static GatheringMapSummary CopyMapSummary(GatheringMapSummary summary, Brush brush)
    {
        return new GatheringMapSummary
        {
            Name = summary.Name,
            ClusterIndex = summary.ClusterIndex,
            TimesGathered = summary.TimesGathered,
            TotalValue = summary.TotalValue,
            ResourceTypeCount = summary.ResourceTypeCount,
            GatheringDurationSeconds = summary.GatheringDurationSeconds,
            ClusterType = summary.ClusterType,
            MostGatheredResource = summary.MostGatheredResource,
            Brush = brush
        };
    }

    private static ObservableCollection<ISeries> CreatePieSeries(IEnumerable<(string Name, double Value, Brush Brush)> values)
    {
        return new ObservableCollection<ISeries>(values.Select(value => new PieSeries<double>
        {
            Name = value.Name,
            Values = [value.Value],
            InnerRadius = 45,
            Fill = ToSolidColorPaint(value.Brush),
            Stroke = null,
            ToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
        }));
    }

    private static Brush GetLocationBrush(int index)
    {
        var palette = new[]
        {
            GatheringResourceType.Wood,
            GatheringResourceType.Fiber,
            GatheringResourceType.Fishing,
            GatheringResourceType.Ore,
            GatheringResourceType.Hide,
            GatheringResourceType.Rock
        };

        return GatheringChartSeriesFilter.GetBrush(palette[index % palette.Length]);
    }

    private static SolidColorPaint CreateChartTextPaint()
    {
        var textBrush = Application.Current?.TryFindResource("SolidColorBrush.Text.1") as SolidColorBrush;
        var color = textBrush?.Color ?? Colors.White;

        return new SolidColorPaint(new SKColor(color.R, color.G, color.B, color.A));
    }

    private static SolidColorPaint ToSolidColorPaint(Brush brush)
    {
        if (brush is not SolidColorBrush solidColorBrush)
        {
            return new SolidColorPaint { Color = SKColors.Transparent };
        }

        var color = solidColorBrush.Color;
        return new SolidColorPaint { Color = new SKColor(color.R, color.G, color.B, color.A) };
    }

    private static void UpdateObservableRangeCollection(ICollection<Gathered> target, IEnumerable<Gathered> source)
    {
        var targetDictionary = target.ToDictionary(x => x.UniqueName);

        foreach (var item in source)
        {
            if (targetDictionary.TryGetValue(item.UniqueName, out var existingItem))
            {
                existingItem.GainedStandardAmount = item.GainedStandardAmount;
                existingItem.GainedBonusAmount = item.GainedBonusAmount;
                existingItem.GainedPremiumBonusAmount = item.GainedPremiumBonusAmount;
                existingItem.GainedTotalAmount = item.GainedTotalAmount;
                existingItem.GainedFame = item.GainedFame;
                existingItem.MiningProcesses = item.MiningProcesses;
                existingItem.EstimatedMarketValue = item.EstimatedMarketValue;
                existingItem.TotalMarketValueWithCulture = item.TotalMarketValueWithCulture;

                targetDictionary.Remove(item.UniqueName);
            }
            else
            {
                target.Add(item);
            }
        }

        foreach (var itemToRemove in targetDictionary.Values)
        {
            target.Remove(itemToRemove);
        }
    }


    private void ResourceChartSeriesFilter_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GatheringChartSeriesFilter.IsSelected))
        {
            return;
        }

        RequestResourceChartUpdate();
    }

    private void RequestResourceChartUpdate()
    {
        lock (_pendingStatsSyncRoot)
        {
            _resourceChartUpdateRequested = true;
        }

        UpdateStats();
    }

    private void UpdateResourceChart()
    {
        if (!_statisticsCache.IsInitialized)
        {
            return;
        }

        var chartBuckets = CreateChartBuckets(SelectedGatheringTimeRange);
        var xAxes = new[]
        {
            new Axis
            {
                LabelsRotation = 15,
                Labels = chartBuckets.Select(x => x.Label).ToArray()
            }
        };

        var selectedSeriesFilters = GatheringStats.ResourceChartSeriesFilters
            .Where(x => x.IsSelected)
            .ToList();

        if (selectedSeriesFilters.Count == 0 || chartBuckets.Count == 0)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                GatheringStats.ResourceChartXAxes = xAxes;
                GatheringStats.ResourceChartSeries = [];
            });
            return;
        }

        var seriesCollection = new ObservableCollection<ISeries>();
        foreach (var selectedSeriesFilter in selectedSeriesFilters)
        {
            var points = new ObservableCollection<ObservablePoint>(
                chartBuckets.Select((chartBucket, index) => new ObservablePoint(
                    index,
                    _statisticsCache.GetChartValue(
                        selectedSeriesFilter.ResourceType,
                        chartBucket.Start,
                        GatheringStats.SelectedResourceChartValueType))));

            seriesCollection.Add(new LineSeries<ObservablePoint>
            {
                Name = selectedSeriesFilter.Name,
                Values = points,
                Fill = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, true),
                Stroke = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, false),
                GeometryStroke = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, false),
                GeometryFill = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, false),
                GeometrySize = 5,
                YToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
            });
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            GatheringStats.ResourceChartXAxes = xAxes;
            GatheringStats.ResourceChartSeries = seriesCollection;
        });
    }

    private static List<ChartBucket> CreateChartBuckets(GatheringTimeRangeOption selectedTimeRange)
    {
        var timeRange = GetTimeRange(selectedTimeRange);
        if (timeRange.BucketCount <= 0)
        {
            return [];
        }

        var buckets = new List<ChartBucket>(timeRange.BucketCount);
        for (var i = 0; i < timeRange.BucketCount; i++)
        {
            var start = AddBuckets(timeRange.Start, i, timeRange.Unit);
            buckets.Add(new ChartBucket(start, GetBucketLabel(start, timeRange.Unit)));
        }

        return buckets;
    }

    private static string GetBucketLabel(DateTime start, GatheringTimeRangeUnit unit)
    {
        return unit switch
        {
            GatheringTimeRangeUnit.Minute => start.ToString("HH:mm", CultureInfo.CurrentCulture),
            GatheringTimeRangeUnit.Hour => start.ToString("dd.MM HH:mm", CultureInfo.CurrentCulture),
            GatheringTimeRangeUnit.Day => start.ToString("d", CultureInfo.CurrentCulture),
            _ => start.ToString("d", CultureInfo.CurrentCulture)
        };
    }

    private static DateTime AlignTimestampToBucketStart(DateTime timestamp, GatheringTimeRangeUnit unit)
    {
        return unit switch
        {
            GatheringTimeRangeUnit.Minute => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0),
            GatheringTimeRangeUnit.Hour => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0),
            GatheringTimeRangeUnit.Day => timestamp.Date,
            _ => timestamp.Date
        };
    }

    private static GatheringTimeRange GetTimeRange(GatheringTimeRangeOption selectedTimeRange)
    {
        if (selectedTimeRange == null || selectedTimeRange.BucketCount <= 0)
        {
            return GatheringTimeRange.Empty;
        }

        var currentBucketStart = AlignTimestampToBucketStart(DateTime.Now, selectedTimeRange.Unit);
        var start = AddBuckets(currentBucketStart, -(selectedTimeRange.BucketCount - 1), selectedTimeRange.Unit);
        var end = AddBuckets(currentBucketStart, 1, selectedTimeRange.Unit);
        return new GatheringTimeRange(start, end, selectedTimeRange.BucketCount, selectedTimeRange.Unit);
    }

    private static DateTime AddBuckets(DateTime bucketStart, int bucketCount, GatheringTimeRangeUnit unit)
    {
        return unit switch
        {
            GatheringTimeRangeUnit.Minute => bucketStart.AddMinutes(bucketCount),
            GatheringTimeRangeUnit.Hour => bucketStart.AddHours(bucketCount),
            GatheringTimeRangeUnit.Day => bucketStart.AddDays(bucketCount),
            _ => bucketStart.AddDays(bucketCount)
        };
    }

    private static SolidColorPaint GetResourceTypeBrush(GatheringResourceType resourceType, bool transparent)
    {
        try
        {
            var resourceKey = transparent ? $"SolidColorBrush.Resource.{resourceType}.Transparent" : $"SolidColorBrush.Resource.{resourceType}";
            var brush = (System.Windows.Media.SolidColorBrush) Application.Current.Resources[resourceKey];
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

    private void GatheringStats_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GatheringStats.SelectedResourceChartValueType))
        {
            return;
        }

        RequestResourceChartUpdate();
    }

    private readonly record struct ChartBucket(DateTime Start, string Label);

    private readonly record struct GatheringTimeRange(DateTime Start, DateTime End, int BucketCount, GatheringTimeRangeUnit Unit)
    {
        public static GatheringTimeRange Empty => new(DateTime.MinValue, DateTime.MinValue, 0, GatheringTimeRangeUnit.Day);
    }

    #region Bindings

    public ObservableRangeCollection<Gathered> GatheredCollection
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
            SettingsController.CurrentSettings.GatheringGridSplitterPosition = field.Value;
            OnPropertyChanged();
        }
    } = GridLength.Auto;

    public GatheringStats GatheringStats
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public SolidColorPaint ChartLegendTextPaint { get; } = CreateChartTextPaint();

    public bool IsGatheringActive
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsGatheringActive = field;
            OnPropertyChanged();
        }
    } = true;

    public ObservableCollection<GatheringTimeRangeOption> GatheringTimeRanges
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public GatheringTimeRangeOption SelectedGatheringTimeRange
    {
        get => _selectedGatheringTimeRange;
        set
        {
            if (ReferenceEquals(_selectedGatheringTimeRange, value))
            {
                return;
            }

            _selectedGatheringTimeRange = value;
            InvalidateStatisticsCache();
            UpdateStats();
            OnPropertyChanged();
        }
    }

    public ObservableCollection<GatheringSessionFilterOption> GatheringSessionFilters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public GatheringSessionFilterOption SelectedGatheringSessionFilter
    {
        get => _selectedGatheringSessionFilter;
        set
        {
            if (ReferenceEquals(_selectedGatheringSessionFilter, value))
            {
                return;
            }

            _selectedGatheringSessionFilter = value;
            InvalidateStatisticsCache();
            UpdateStats();
            OnPropertyChanged();
        }
    }

    public int SelectedGatheringTabIndex
    {
        get => _selectedGatheringTabIndex;
        set
        {
            if (_selectedGatheringTabIndex == value)
            {
                return;
            }

            _selectedGatheringTabIndex = value;
            if (value == 0)
            {
                MarkOverviewDirty();
                UpdateStats();
            }

            OnPropertyChanged();
        }
    }

    public void SetViewVisibility(bool isVisible)
    {
        if (_isGatheringViewVisible == isVisible)
        {
            return;
        }

        _isGatheringViewVisible = isVisible;
        if (isVisible)
        {
            MarkOverviewDirty();
            UpdateStats();
        }
    }

    public void RefreshSessionFilters(
        IReadOnlyCollection<Gathered> gatheredEntries,
        Guid activeSessionId,
        DateTime activeSessionStartedAtUtc)
    {
        var selectedSessionId = SelectedGatheringSessionFilter?.SessionId;
        var filters = new List<GatheringSessionFilterOption>
        {
            new(null, LocalizationController.Translation("ALL_SESSIONS")),
            new(
                activeSessionId,
                CreateSessionFilterName(activeSessionStartedAtUtc, true),
                activeSessionStartedAtUtc)
        };

        filters.AddRange(gatheredEntries
            .Where(x => x.SessionId != activeSessionId)
            .GroupBy(x => x.SessionId)
            .Select(group => new
            {
                SessionId = group.Key,
                StartedAtUtc = group.Min(x => x.TimestampDateTimeUtc),
                EndedAtUtc = group.Max(x => x.TimestampDateTimeUtc)
            })
            .OrderByDescending(x => x.StartedAtUtc)
            .Select(x => new GatheringSessionFilterOption(
                x.SessionId,
                CreateSessionFilterName(x.StartedAtUtc, false),
                x.StartedAtUtc,
                x.EndedAtUtc,
                true)));

        GatheringSessionFilters = new ObservableCollection<GatheringSessionFilterOption>(filters);
        SelectedGatheringSessionFilter = filters.FirstOrDefault(x => x.SessionId == selectedSessionId) ?? filters[0];
    }

    private static string CreateSessionFilterName(DateTime startedAtUtc, bool isActive)
    {
        var activeMarker = isActive ? "* " : string.Empty;
        return $"{activeMarker}{startedAtUtc.ToLocalTime():g}";
    }

    #endregion

    public static string TranslationGatheringActive => LocalizationController.Translation("GATHERING_ACTIVE");
    public static string TranslationGathering => LocalizationController.Translation("GATHERING");
    public static string TranslationTimeRange => LocalizationController.Translation("TIME_RANGE");
    public static string TranslationSession => LocalizationController.Translation("SESSION");
    public static string TranslationResetSession => LocalizationController.Translation("RESET_SESSION");
    public static string TranslationDeleteSession => LocalizationController.Translation("DELETE_SESSION");
    public static string TranslationGenerally => LocalizationController.Translation("GENERALLY");
    public static string TranslationWood => LocalizationController.Translation("WOOD");
    public static string TranslationFiber => LocalizationController.Translation("FIBER");
    public static string TranslationHide => LocalizationController.Translation("HIDE");
    public static string TranslationOre => LocalizationController.Translation("ORE");
    public static string TranslationRock => LocalizationController.Translation("ROCK");
    public static string TranslationFishing => LocalizationController.Translation("FISHING");
}