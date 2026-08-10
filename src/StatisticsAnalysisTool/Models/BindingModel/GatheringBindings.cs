using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Serilog;
using SkiaSharp;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
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
    private readonly SemaphoreSlim _statsUpdateSemaphore = new(1, 1);
    private GatheringTimeRangeOption _selectedGatheringTimeRange;
    private GatheringSessionFilterOption _selectedGatheringSessionFilter;
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
        UpdateStats();
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
            var gatherCollection = await Application.Current.Dispatcher.InvokeAsync(() => GatheredCollection.ToList());
            var filteredGatherCollection = GetFilteredGatheredEntries(gatherCollection);

            var hideEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Hide);
            var oreEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Ore);
            var fiberEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Fiber);
            var woodEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Wood);
            var rockEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Rock);
            var fishEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Fishing);

            var hideTask = GroupAndSumAsync(hideEntries);
            var oreTask = GroupAndSumAsync(oreEntries);
            var fiberTask = GroupAndSumAsync(fiberEntries);
            var woodTask = GroupAndSumAsync(woodEntries);
            var rockTask = GroupAndSumAsync(rockEntries);
            var fishTask = GroupAndSumAsync(fishEntries, true);

            await Task.WhenAll(hideTask, oreTask, fiberTask, woodTask, rockTask, fishTask);

            var hide = await hideTask.ConfigureAwait(false);
            var ore = await oreTask.ConfigureAwait(false);
            var fiber = await fiberTask.ConfigureAwait(false);
            var wood = await woodTask.ConfigureAwait(false);
            var rock = await rockTask.ConfigureAwait(false);
            var fish = await fishTask.ConfigureAwait(false);

            var gainedSilverByHide = hide.Sum(x => x.TotalMarketValue.IntegerValue);
            var gainedSilverPerHourByHide = CalculateSilverPerHour(hideEntries);
            var gainedSilverByOre = ore.Sum(x => x.TotalMarketValue.IntegerValue);
            var gainedSilverPerHourByOre = CalculateSilverPerHour(oreEntries);
            var gainedSilverByFiber = fiber.Sum(x => x.TotalMarketValue.IntegerValue);
            var gainedSilverPerHourByFiber = CalculateSilverPerHour(fiberEntries);
            var gainedSilverByWood = wood.Sum(x => x.TotalMarketValue.IntegerValue);
            var gainedSilverPerHourByWood = CalculateSilverPerHour(woodEntries);
            var gainedSilverByRock = rock.Sum(x => x.TotalMarketValue.IntegerValue);
            var gainedSilverPerHourByRock = CalculateSilverPerHour(rockEntries);
            var gainedSilverByFish = fish.Sum(x => x.TotalMarketValue.IntegerValue);
            var gainedSilverPerHourByFish = CalculateSilverPerHour(fishEntries);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                // Hide
                UpdateObservableRangeCollection(GatheringStats.GatheredHide, hide);
                GatheringStats.GainedSilverByHide = gainedSilverByHide;
                GatheringStats.GainedSilverPerHourByHide = gainedSilverPerHourByHide;

                // Ore
                UpdateObservableRangeCollection(GatheringStats.GatheredOre, ore);
                GatheringStats.GainedSilverByOre = gainedSilverByOre;
                GatheringStats.GainedSilverPerHourByOre = gainedSilverPerHourByOre;

                // Fiber
                UpdateObservableRangeCollection(GatheringStats.GatheredFiber, fiber);
                GatheringStats.GainedSilverByFiber = gainedSilverByFiber;
                GatheringStats.GainedSilverPerHourByFiber = gainedSilverPerHourByFiber;

                // Wood
                UpdateObservableRangeCollection(GatheringStats.GatheredWood, wood);
                GatheringStats.GainedSilverByWood = gainedSilverByWood;
                GatheringStats.GainedSilverPerHourByWood = gainedSilverPerHourByWood;

                // Rock
                UpdateObservableRangeCollection(GatheringStats.GatheredRock, rock);
                GatheringStats.GainedSilverByRock = gainedSilverByRock;
                GatheringStats.GainedSilverPerHourByRock = gainedSilverPerHourByRock;

                // Fish
                UpdateObservableRangeCollection(GatheringStats.GatheredFish, fish);
                GatheringStats.GainedSilverByFish = gainedSilverByFish;
                GatheringStats.GainedSilverPerHourByFish = gainedSilverPerHourByFish;
            });

            // Most gathered resource
            var mostGatheredResource = filteredGatherCollection.Count > 0
                ? filteredGatherCollection
                    .GroupBy(x => x.UniqueName)
                    .Select(g => new Gathered
                    {
                        UniqueName = g.Key,
                        GainedStandardAmount = g.Sum(x => x.GainedStandardAmount),
                        GainedBonusAmount = g.Sum(x => x.GainedBonusAmount),
                        GainedPremiumBonusAmount = g.Sum(x => x.GainedPremiumBonusAmount),
                        GainedTotalAmount = g.Sum(x => x.GainedTotalAmount),
                        MiningProcesses = g.Sum(x => x.MiningProcesses)
                    })
                    .MaxBy(x => x.GainedTotalAmount)
                : null;

            // Most gathered cluster
            var mostGatheredCluster = filteredGatherCollection.Count > 0
                ? filteredGatherCollection
                    .GroupBy(x => x.ClusterIndex)
                    .Select(g => new Gathered
                    {
                        ClusterIndex = g.Key,
                        GainedStandardAmount = g.Sum(x => x.GainedStandardAmount),
                        GainedBonusAmount = g.Sum(x => x.GainedBonusAmount),
                        GainedPremiumBonusAmount = g.Sum(x => x.GainedPremiumBonusAmount),
                        GainedTotalAmount = g.Sum(x => x.GainedTotalAmount),
                        MiningProcesses = g.Sum(x => x.MiningProcesses)
                    })
                    .MaxBy(x => x.MiningProcesses)
                : null;

            // Most total resources
            var totalResources = filteredGatherCollection
                .Sum(x => x.GainedTotalAmount);

            // Most total mining processes
            var totalMiningProcesses = filteredGatherCollection
                .Sum(GetGatheringProcessCount);

            // Total gained silver
            var totalGainedSilver = filteredGatherCollection
                .Sum(x => x.TotalMarketValue.IntegerValue);
            var totalGainedSilverPerHour = CalculateSilverPerHour(filteredGatherCollection);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                GatheringStats.MostGatheredResource = mostGatheredResource;
                GatheringStats.MostGatheredCluster = mostGatheredCluster;
                GatheringStats.TotalResources = totalResources;
                GatheringStats.TotalMiningProcesses = totalMiningProcesses;
                GatheringStats.TotalGainedSilverString = totalGainedSilver;
                GatheringStats.TotalGainedSilverPerHour = totalGainedSilverPerHour;
            });

            UpdateResourceChart(filteredGatherCollection);
            await UpdateGatheringOverviewAsync(filteredGatherCollection);
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private List<Gathered> GetFilteredGatheredEntries(IEnumerable<Gathered> gatheredData)
    {
        var timeRange = GetTimeRange(SelectedGatheringTimeRange);
        var selectedSession = SelectedGatheringSessionFilter;

        return gatheredData
            .Where(x => timeRange.Contains(x.TimestampDateTimeUtc.ToLocalTime()))
            .Where(x => selectedSession == null || selectedSession.Contains(x.SessionId))
            .ToList();
    }

    private async Task UpdateGatheringOverviewAsync(IReadOnlyCollection<Gathered> gatheredData)
    {
        var totalValue = gatheredData.Sum(x => x.TotalMarketValue.IntegerValue);
        var totalAmount = gatheredData.Sum(x => (long) x.GainedTotalAmount);
        var resourceSummaries = gatheredData
            .Where(x => !string.IsNullOrWhiteSpace(x.UniqueName))
            .GroupBy(x => x.UniqueName)
            .Select(CreateResourceSummary)
            .ToList();
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
        var mapSummaries = gatheredData
            .GroupBy(x => new { x.ClusterIndex, x.MapType, x.InstanceName })
            .Select(group => CreateMapSummary(group.Key.ClusterIndex, group.Key.MapType, group.Key.InstanceName, group))
            .OrderByDescending(x => x.TotalValue)
            .ThenByDescending(x => x.TimesGathered)
            .ToList();
        var bestMap = mapSummaries.FirstOrDefault();
        var resourceTypeSummaries = CreateResourceTypeSummaries(gatheredData, totalValue);
        var locationSummaries = CreateLocationSummaries(mapSummaries);
        var recentGatherings = gatheredData
            .OrderByDescending(x => x.TimestampUtc)
            .Take(5)
            .ToList();
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
            GatheringStats.BestSingleGatheringValue = gatheredData.Count > 0
                ? gatheredData.Max(x => x.TotalMarketValue.IntegerValue)
                : 0;
            GatheringStats.GatheringDurationSeconds = GetGatheringDurationSeconds(gatheredData);
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

    private static GatheringResourceSummary CreateResourceSummary(IGrouping<string, Gathered> group)
    {
        var entries = group.ToList();
        var timesGathered = entries.Sum(GetGatheringProcessCount);
        var totalValue = entries.Sum(x => x.TotalMarketValue.IntegerValue);

        return new GatheringResourceSummary
        {
            UniqueName = group.Key,
            Item = ItemController.GetItemByUniqueName(group.Key),
            TimesGathered = timesGathered,
            TotalAmount = entries.Sum(x => (long) x.GainedTotalAmount),
            TotalValue = totalValue,
            AverageValuePerGather = timesGathered > 0 ? (double) totalValue / timesGathered : 0,
            GatheringDurationSeconds = GetGatheringDurationSeconds(entries),
            TopLocation = entries
                .GroupBy(x => x.ClusterUniqueName)
                .OrderByDescending(x => x.Sum(entry => entry.TotalMarketValue.IntegerValue))
                .Select(x => x.Key)
                .FirstOrDefault() ?? string.Empty
        };
    }

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

    private static GatheringMapSummary CreateMapSummary(
        string clusterIndex,
        MapType mapType,
        string instanceName,
        IEnumerable<Gathered> gatheredData)
    {
        var entries = gatheredData.ToList();
        var name = ClusterController.ComposingMapInfoString(clusterIndex, mapType, instanceName).Trim();
        var mostGatheredResourceUniqueName = entries
            .Where(x => !string.IsNullOrWhiteSpace(x.UniqueName))
            .GroupBy(x => x.UniqueName)
            .OrderByDescending(group => group.Sum(x => (long) x.GainedTotalAmount))
            .ThenByDescending(group => group.Sum(x => x.TotalMarketValue.IntegerValue))
            .Select(group => group.Key)
            .FirstOrDefault();
        var mostGatheredResource = string.IsNullOrWhiteSpace(mostGatheredResourceUniqueName)
            ? null
            : ItemController.GetItemByUniqueName(mostGatheredResourceUniqueName);

        return new GatheringMapSummary
        {
            Name = string.IsNullOrWhiteSpace(name) ? LocalizationController.Translation("NO_DATA") : name,
            ClusterIndex = clusterIndex ?? string.Empty,
            TimesGathered = entries.Sum(GetGatheringProcessCount),
            TotalValue = entries.Sum(x => x.TotalMarketValue.IntegerValue),
            ResourceTypeCount = entries.Select(GetGatheringResourceType).Where(x => x != GatheringResourceType.Unknown).Distinct().Count(),
            GatheringDurationSeconds = GetGatheringDurationSeconds(entries),
            ClusterType = WorldData.GetClusterTypeByIndex(clusterIndex),
            MostGatheredResource = mostGatheredResource
        };
    }

    private static List<GatheringResourceTypeSummary> CreateResourceTypeSummaries(
        IReadOnlyCollection<Gathered> gatheredData,
        long totalValue)
    {
        var resourceTypes = new[]
        {
            GatheringResourceType.Wood,
            GatheringResourceType.Fiber,
            GatheringResourceType.Fishing,
            GatheringResourceType.Ore,
            GatheringResourceType.Hide,
            GatheringResourceType.Rock
        };
        var entriesByResourceType = gatheredData
            .GroupBy(GetGatheringResourceType)
            .Where(group => group.Key != GatheringResourceType.Unknown)
            .ToDictionary(group => group.Key, group => group.ToList());
        var summaries = new List<GatheringResourceTypeSummary>();

        foreach (var resourceType in resourceTypes)
        {
            if (!entriesByResourceType.TryGetValue(resourceType, out var entries))
            {
                continue;
            }

            var value = entries.Sum(x => x.TotalMarketValue.IntegerValue);
            summaries.Add(new GatheringResourceTypeSummary
            {
                ResourceType = resourceType,
                Name = GetResourceTypeName(resourceType),
                Amount = entries.Sum(x => (long) x.GainedTotalAmount),
                Value = value,
                SharePercentage = totalValue > 0 ? (double) value / totalValue * 100 : 0,
                Brush = GatheringChartSeriesFilter.GetBrush(resourceType)
            });
        }

        return summaries.OrderByDescending(x => x.Value).ToList();
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

    private static long GetGatheringProcessCount(Gathered gathered)
    {
        return gathered.HasBeenFished ? 1 : gathered.MiningProcesses;
    }

    private static double GetGatheringDurationSeconds(IEnumerable<Gathered> gatheredData)
    {
        var timestamps = gatheredData.Select(x => x.TimestampDateTimeUtc).OrderBy(x => x).ToList();
        return timestamps.Count > 1 ? (timestamps[^1] - timestamps[0]).TotalSeconds : 0;
    }

    private static string GetResourceTypeName(GatheringResourceType resourceType)
    {
        return LocalizationController.Translation(resourceType.ToString().ToUpperInvariant());
    }

    private static List<Gathered> FilterGatheredEntries(IEnumerable<Gathered> gatheredData, GatheringResourceType resourceType)
    {
        return gatheredData
            .Where(x => GetGatheringResourceType(x) == resourceType)
            .ToList();
    }

    private static async Task<List<Gathered>> GroupAndSumAsync(IEnumerable<Gathered> gatheredData, bool hasBeenFished = false)
    {
        try
        {
            return await Task.Run(() =>
            {
                var groupedData = gatheredData.GroupBy(x => x.UniqueName)
                    .Select(g => new Gathered()
                    {
                        UniqueName = g.Key,
                        EstimatedMarketValue = FixPoint.FromInternalValue(g.FirstOrDefault()?.EstimatedMarketValue.InternalValue ?? 0),
                        GainedStandardAmount = g.Sum(x => x.GainedStandardAmount),
                        GainedBonusAmount = g.Sum(x => x.GainedBonusAmount),
                        GainedPremiumBonusAmount = g.Sum(x => x.GainedPremiumBonusAmount),
                        GainedTotalAmount = g.Sum(x => x.GainedTotalAmount),
                        GainedFame = g.Sum(x => x.GainedFame),
                        MiningProcesses = g.Sum(x => x.MiningProcesses),
                        HasBeenFished = hasBeenFished
                    }).ToList();

                return groupedData;
            }) ?? new List<Gathered>();
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return new List<Gathered>();
        }
    }

    private static double CalculateSilverPerHour(IEnumerable<Gathered> gatheredData)
    {
        var gatheredEntries = gatheredData
            .OrderBy(x => x.TimestampDateTimeUtc)
            .ToList();

        if (gatheredEntries.Count == 0)
        {
            return 0;
        }

        var totalSilver = gatheredEntries.Sum(x => x.TotalMarketValue.IntegerValue);
        var durationInSeconds = Math.Max(3600d, (gatheredEntries[^1].TimestampDateTimeUtc - gatheredEntries[0].TimestampDateTimeUtc).TotalSeconds);

        return ((double) totalSilver).GetValuePerHour(durationInSeconds);
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

        UpdateResourceChart();
    }

    private void UpdateResourceChart()
    {
        var filteredGatherCollection = GetFilteredGatheredEntries(GatheredCollection.ToList());
        UpdateResourceChart(filteredGatherCollection);
    }

    private void UpdateResourceChart(IEnumerable<Gathered> gatheredData)
    {
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
            var valuesLookup = gatheredData
                .Where(x => GetGatheringResourceType(x) == selectedSeriesFilter.ResourceType)
                .GroupBy(x => AlignTimestampToBucketStart(x.TimestampDateTimeUtc.ToLocalTime(), SelectedGatheringTimeRange.Unit))
                .ToDictionary(x => x.Key, x => GetChartMetricValue(x, GatheringStats.SelectedResourceChartValueType));

            var points = new ObservableCollection<ObservablePoint>();

            for (var i = 0; i < chartBuckets.Count; i++)
            {
                var chartBucket = chartBuckets[i];
                var value = valuesLookup.GetValueOrDefault(chartBucket.Start);
                points.Add(new ObservablePoint(i, value));
            }

            var lineSeries = new LineSeries<ObservablePoint>
            {
                Name = selectedSeriesFilter.Name,
                Values = points,
                Fill = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, true),
                Stroke = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, false),
                GeometryStroke = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, false),
                GeometryFill = GetResourceTypeBrush(selectedSeriesFilter.ResourceType, false),
                GeometrySize = 5,
                YToolTipLabelFormatter = chartPoint => chartPoint.Coordinate.PrimaryValue.ToChartTooltipNumberString()
            };

            seriesCollection.Add(lineSeries);
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

    private static GatheringResourceType GetGatheringResourceType(Gathered gathered)
    {
        if (gathered == null)
        {
            return GatheringResourceType.Unknown;
        }

        if (gathered.HasBeenFished)
        {
            return GatheringResourceType.Fishing;
        }

        return gathered.Item?.FullItemInformation?.ShopSubCategory2?.ToLowerInvariant() switch
        {
            "wood" => GatheringResourceType.Wood,
            "fiber" => GatheringResourceType.Fiber,
            "hide" => GatheringResourceType.Hide,
            "ore" => GatheringResourceType.Ore,
            "rock" => GatheringResourceType.Rock,
            _ => GatheringResourceType.Unknown
        };
    }

    private static double GetChartMetricValue(IEnumerable<Gathered> gatheredEntries, GatheringChartValueType chartValueType)
    {
        return chartValueType switch
        {
            GatheringChartValueType.ResourceSilverValue => gatheredEntries.Sum(x => (double) x.TotalMarketValue.IntegerValue),
            _ => gatheredEntries.Sum(x => (double) x.GainedTotalAmount)
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

        UpdateResourceChart();
    }

    private readonly record struct ChartBucket(DateTime Start, string Label);

    private readonly record struct GatheringTimeRange(DateTime Start, DateTime End, int BucketCount, GatheringTimeRangeUnit Unit)
    {
        public static GatheringTimeRange Empty => new(DateTime.MinValue, DateTime.MinValue, 0, GatheringTimeRangeUnit.Day);

        public bool Contains(DateTime timestamp)
        {
            return timestamp >= Start && timestamp < End;
        }
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
            UpdateStats();
            OnPropertyChanged();
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