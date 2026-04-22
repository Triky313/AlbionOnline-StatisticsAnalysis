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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class GatheringBindings : BaseViewModel
{
    private bool _isGatheringActive = true;
    private GatheringStats _gatheringStats = new();
    private ObservableRangeCollection<Gathered> _gatheredCollection = new();
    private readonly ListCollectionView _gatheredCollectionView;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private Dictionary<GatheringFilterType, string> _gatheringFilter = new()
    {
        { GatheringFilterType.Generally, LocalizationController.Translation("GENERALLY") },
        { GatheringFilterType.Wood, LocalizationController.Translation("WOOD") },
        { GatheringFilterType.Fiber, LocalizationController.Translation("FIBER") },
        { GatheringFilterType.Hide, LocalizationController.Translation("HIDE") },
        { GatheringFilterType.Ore, LocalizationController.Translation("ORE") },
        { GatheringFilterType.Rock, LocalizationController.Translation("ROCK") },
        { GatheringFilterType.Fishing, LocalizationController.Translation("FISHING") }
    };
    private GatheringFilterType _selectedGatheringFilter = GatheringFilterType.Generally;
    private GatheringStatsTimeType _gatheringStatsTimeTypeSelection = GatheringStatsTimeType.Today;
    private AutoDeleteGatheringStats _autoDeleteStatsByDateSelection;

    public GatheringBindings()
    {
        IsGatheringActive = SettingsController.CurrentSettings.IsGatheringActive;
        AutoDeleteStatsByDateSelection = SettingsController.CurrentSettings.AutoDeleteGatheringStats;

        foreach (var resourceChartSeriesFilter in GatheringStats.ResourceChartSeriesFilters)
        {
            resourceChartSeriesFilter.PropertyChanged += ResourceChartSeriesFilter_PropertyChanged;
        }

        GatheringStats.PropertyChanged += GatheringStats_PropertyChanged;

        GatheredCollectionView = CollectionViewSource.GetDefaultView(GatheredCollection) as ListCollectionView;

        if (GatheredCollectionView != null)
        {
            GatheredCollectionView.IsLiveSorting = true;
            GatheredCollectionView.IsLiveFiltering = true;
            GatheredCollectionView.CustomSort = new GatheredComparer();
        }

        GatheredCollection.CollectionChanged += UpdateStatsAsync;
    }

    public void UpdateStats()
    {
        UpdateStatsAsync(null, null);
    }

    public async void UpdateStatsAsync(object sender, NotifyCollectionChangedEventArgs e)
    {
        try
        {
            var gatherCollection = GatheredCollection.ToList();
            var filteredGatherCollection = GetGatheredEntriesByTimeFilter(gatherCollection, GatheringStatsTimeTypeSelection);

            var hideEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Hide);
            var oreEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Ore);
            var fiberEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Fiber);
            var woodEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Wood);
            var rockEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Rock);
            var fishEntries = FilterGatheredEntries(filteredGatherCollection, GatheringResourceType.Fishing);

            // Hide
            var hide = await GroupAndSumAsync(hideEntries);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateObservableRangeCollection(GatheringStats.GatheredHide, hide);
                GatheringStats.GainedSilverByHide = hide.Sum(x => x.TotalMarketValue.IntegerValue);
                GatheringStats.GainedSilverPerHourByHide = CalculateSilverPerHour(hideEntries);
            });

            // Ore
            var ore = await GroupAndSumAsync(oreEntries);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateObservableRangeCollection(GatheringStats.GatheredOre, ore);
                GatheringStats.GainedSilverByOre = ore.Sum(x => x.TotalMarketValue.IntegerValue);
                GatheringStats.GainedSilverPerHourByOre = CalculateSilverPerHour(oreEntries);
            });

            // Fiber
            var fiber = await GroupAndSumAsync(fiberEntries);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateObservableRangeCollection(GatheringStats.GatheredFiber, fiber);
                GatheringStats.GainedSilverByFiber = fiber.Sum(x => x.TotalMarketValue.IntegerValue);
                GatheringStats.GainedSilverPerHourByFiber = CalculateSilverPerHour(fiberEntries);
            });

            // Wood
            var wood = await GroupAndSumAsync(woodEntries);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateObservableRangeCollection(GatheringStats.GatheredWood, wood);
                GatheringStats.GainedSilverByWood = wood.Sum(x => x.TotalMarketValue.IntegerValue);
                GatheringStats.GainedSilverPerHourByWood = CalculateSilverPerHour(woodEntries);
            });

            // Rock
            var rock = await GroupAndSumAsync(rockEntries);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateObservableRangeCollection(GatheringStats.GatheredRock, rock);
                GatheringStats.GainedSilverByRock = rock.Sum(x => x.TotalMarketValue.IntegerValue);
                GatheringStats.GainedSilverPerHourByRock = CalculateSilverPerHour(rockEntries);
            });

            // Fish
            var fish = await GroupAndSumAsync(fishEntries, true);
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                UpdateObservableRangeCollection(GatheringStats.GatheredFish, fish);
                GatheringStats.GainedSilverByFish = fish.Sum(x => x.TotalMarketValue.IntegerValue);
                GatheringStats.GainedSilverPerHourByFish = CalculateSilverPerHour(fishEntries);
            });

            // Most gathered resource
            if (filteredGatherCollection.Any(x => x?.GainedTotalAmount != null))
            {
                var mostGatheredResource = filteredGatherCollection
                    .GroupBy(x => x.UniqueName)
                    .Select(g => new Gathered
                    {
                        UniqueName = g.Key,
                        GainedStandardAmount = g.Sum(x => x?.GainedStandardAmount ?? 0),
                        GainedBonusAmount = g.Sum(x => x?.GainedBonusAmount ?? 0),
                        GainedPremiumBonusAmount = g.Sum(x => x?.GainedPremiumBonusAmount ?? 0),
                        GainedTotalAmount = g.Sum(x => x?.GainedTotalAmount ?? 0),
                        MiningProcesses = g.Sum(x => x?.MiningProcesses ?? 0)
                    })
                    .MaxBy(x => x.GainedTotalAmount);

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    GatheringStats.MostGatheredResource = mostGatheredResource;
                });
            }

            // Most gathered cluster
            var mostGatheredCluster = filteredGatherCollection
                .GroupBy(x => x.ClusterIndex)
                .Select(g => new Gathered
                {
                    ClusterIndex = g.Key,
                    GainedStandardAmount = g.Sum(x => x.GainedStandardAmount),
                    GainedBonusAmount = g.Sum(x => x.GainedBonusAmount),
                    GainedPremiumBonusAmount = g.Sum(x => x.GainedPremiumBonusAmount),
                    GainedTotalAmount = g.Sum(x => x.GainedTotalAmount),
                    MiningProcesses = g.Sum(x => x.MiningProcesses)
                }).MaxBy(x => x.MiningProcesses);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                GatheringStats.MostGatheredCluster = mostGatheredCluster;
            });

            // Most total resources
            var totalResources = filteredGatherCollection
                .Sum(x => x.GainedTotalAmount);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                GatheringStats.TotalResources = totalResources;
            });

            // Most total mining processes
            var totalMiningProcesses = filteredGatherCollection
                .Sum(x => x.MiningProcesses);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                GatheringStats.TotalMiningProcesses = totalMiningProcesses;
            });

            // Total gained silver
            var totalGainedSilver = filteredGatherCollection
                .Sum(x => x.TotalMarketValue.IntegerValue);

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                GatheringStats.TotalGainedSilverString = totalGainedSilver;
                GatheringStats.TotalGainedSilverPerHour = CalculateSilverPerHour(filteredGatherCollection);
            });

            UpdateResourceChart(filteredGatherCollection);
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static List<Gathered> GetGatheredEntriesByTimeFilter(IEnumerable<Gathered> gatheredData, GatheringStatsTimeType gatheringStatsTimeType)
    {
        if (gatheringStatsTimeType == GatheringStatsTimeType.Total)
        {
            return gatheredData.ToList();
        }

        var timeRange = GetTimeRange(gatheringStatsTimeType);
        return gatheredData
            .Where(x => timeRange.Contains(x.TimestampDateTimeUtc))
            .ToList();
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

    public static bool IsTimestampOkayByGatheringStatsTimeType(DateTime dateTime, GatheringStatsTimeType gatheringStatsTimeType)
    {
        if (gatheringStatsTimeType == GatheringStatsTimeType.Total)
        {
            return true;
        }

        return GetTimeRange(gatheringStatsTimeType).Contains(dateTime);
    }

    public async Task RemoveResourcesByIdsAsync(IEnumerable<Guid> guids)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var gathered in GatheredCollection?.ToList().Where(x => guids.Contains(x.Guid)) ?? new List<Gathered>())
            {
                GatheredCollection?.Remove(gathered);
            }

            UpdateStats();
        });
    }

    private void ResourceChartSeriesFilter_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GatheringChartSeriesFilter.IsSelected))
        {
            return;
        }

        UpdateStats();
    }

    private void UpdateResourceChart(IEnumerable<Gathered> gatheredData)
    {
        var chartBuckets = CreateChartBuckets(GatheringStatsTimeTypeSelection);
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
                .GroupBy(x => AlignTimestampToBucketStart(x.TimestampDateTimeUtc, GatheringStatsTimeTypeSelection))
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

    private static List<ChartBucket> CreateChartBuckets(GatheringStatsTimeType gatheringStatsTimeType)
    {
        var timeRange = GetTimeRange(gatheringStatsTimeType);
        if (timeRange.BucketCount <= 0)
        {
            return [];
        }

        var buckets = new List<ChartBucket>(timeRange.BucketCount);
        for (var i = 0; i < timeRange.BucketCount; i++)
        {
            var start = timeRange.Start.AddTicks(timeRange.BucketSize.Ticks * i);
            buckets.Add(new ChartBucket(start, GetBucketLabel(start, i, timeRange.BucketCount, gatheringStatsTimeType)));
        }

        return buckets;
    }

    private static string GetBucketLabel(DateTime start, int index, int totalBucketCount, GatheringStatsTimeType gatheringStatsTimeType)
    {
        return gatheringStatsTimeType switch
        {
            GatheringStatsTimeType.Today => index % 2 == 0 || index == totalBucketCount - 1 ? start.ToString("HH:mm", CultureInfo.CurrentCulture) : string.Empty,
            GatheringStatsTimeType.ThisWeek or GatheringStatsTimeType.LastWeek => start.ToString("ddd", CultureInfo.CurrentCulture),
            GatheringStatsTimeType.Month => index % 5 == 0 || index == totalBucketCount - 1 ? start.ToString("dd.MM", CultureInfo.CurrentCulture) : string.Empty,
            GatheringStatsTimeType.Year => start.Day == 1 || index == 0 || index == totalBucketCount - 1 ? start.ToString("MMM yy", CultureInfo.CurrentCulture) : string.Empty,
            _ => start.ToString("g", CultureInfo.CurrentCulture)
        };
    }

    private static DateTime AlignTimestampToBucketStart(DateTime timestamp, GatheringStatsTimeType gatheringStatsTimeType)
    {
        return gatheringStatsTimeType switch
        {
            GatheringStatsTimeType.Today => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0),
            GatheringStatsTimeType.ThisWeek => timestamp.Date,
            GatheringStatsTimeType.LastWeek => timestamp.Date,
            GatheringStatsTimeType.Month => timestamp.Date,
            GatheringStatsTimeType.Year => timestamp.Date,
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

    private static GatheringTimeRange GetTimeRange(GatheringStatsTimeType gatheringStatsTimeType)
    {
        var currentDay = DateTime.UtcNow.Date;
        var startOfThisWeek = GetStartOfIsoWeek(currentDay);

        return gatheringStatsTimeType switch
        {
            GatheringStatsTimeType.Today => new GatheringTimeRange(currentDay, currentDay.AddDays(1), 24, TimeSpan.FromHours(1)),
            GatheringStatsTimeType.ThisWeek => new GatheringTimeRange(startOfThisWeek, startOfThisWeek.AddDays(7), 7, TimeSpan.FromDays(1)),
            GatheringStatsTimeType.LastWeek => new GatheringTimeRange(startOfThisWeek.AddDays(-7), startOfThisWeek, 7, TimeSpan.FromDays(1)),
            GatheringStatsTimeType.Month => new GatheringTimeRange(currentDay.AddDays(-29), currentDay.AddDays(1), 30, TimeSpan.FromDays(1)),
            GatheringStatsTimeType.Year => new GatheringTimeRange(currentDay.AddDays(-364), currentDay.AddDays(1), 365, TimeSpan.FromDays(1)),
            _ => GatheringTimeRange.Empty
        };
    }

    private static DateTime GetStartOfIsoWeek(DateTime dateTime)
    {
        var diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dateTime.AddDays(-diff).Date;
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

        UpdateStats();
    }

    private readonly record struct ChartBucket(DateTime Start, string Label);

    private readonly record struct GatheringTimeRange(DateTime Start, DateTime End, int BucketCount, TimeSpan BucketSize)
    {
        public static GatheringTimeRange Empty => new(DateTime.MinValue, DateTime.MinValue, 0, TimeSpan.Zero);

        public bool Contains(DateTime timestamp)
        {
            return timestamp >= Start && timestamp < End;
        }
    }

    #region Bindings

    public ListCollectionView GatheredCollectionView
    {
        get => _gatheredCollectionView;
        init
        {
            _gatheredCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredCollection
    {
        get => _gatheredCollection;
        set
        {
            _gatheredCollection = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.GatheringGridSplitterPosition = _gridSplitterPosition.Value;
            OnPropertyChanged();
        }
    }

    public GatheringStats GatheringStats
    {
        get => _gatheringStats;
        set
        {
            _gatheringStats = value;
            OnPropertyChanged();
        }
    }

    public bool IsGatheringActive
    {
        get => _isGatheringActive;
        set
        {
            _isGatheringActive = value;
            SettingsController.CurrentSettings.IsGatheringActive = _isGatheringActive;
            OnPropertyChanged();
        }
    }

    public Dictionary<GatheringFilterType, string> GatheringFilter
    {
        get => _gatheringFilter;
        set
        {
            _gatheringFilter = value;
            OnPropertyChanged();
        }
    }

    public GatheringFilterType SelectedGatheringFilter
    {
        get => _selectedGatheringFilter;
        set
        {
            _selectedGatheringFilter = value;
            GatheringStats.GatheringFilterType = value;
            UpdateStats();
            OnPropertyChanged();
        }
    }

    public List<GatheringStatsFilterStruct> GatheringStatsTimeTypes { get; } = new()
    {
        new GatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("TODAY"),
            GatheringStatsTimeType = GatheringStatsTimeType.Today
        },
        new GatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("THIS_WEEK"),
            GatheringStatsTimeType = GatheringStatsTimeType.ThisWeek
        },
        new GatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("LAST_WEEK"),
            GatheringStatsTimeType = GatheringStatsTimeType.LastWeek
        },
        new GatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("LAST_30_DAYS"),
            GatheringStatsTimeType = GatheringStatsTimeType.Month
        },
        new GatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("LAST_365_DAYS"),
            GatheringStatsTimeType = GatheringStatsTimeType.Year
        }
    };

    public GatheringStatsTimeType GatheringStatsTimeTypeSelection
    {
        get => _gatheringStatsTimeTypeSelection;
        set
        {
            _gatheringStatsTimeTypeSelection = value;
            UpdateStats();
            OnPropertyChanged();
        }
    }

    public List<AutoDeleteGatheringStatsFilterStruct> AutoDeleteStatsByDate { get; } = new()
    {
        new AutoDeleteGatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("NEVER_DELETE"),
            AutoDeleteGatheringStats = AutoDeleteGatheringStats.NeverDelete
        },
        new AutoDeleteGatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("DELETE_AFTER_7_DAYS"),
            AutoDeleteGatheringStats = AutoDeleteGatheringStats.DeleteAfter7Days
        },
        new AutoDeleteGatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("DELETE_AFTER_14_DAYS"),
            AutoDeleteGatheringStats = AutoDeleteGatheringStats.DeleteAfter14Days
        },
        new AutoDeleteGatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("DELETE_AFTER_30_DAYS"),
            AutoDeleteGatheringStats = AutoDeleteGatheringStats.DeleteAfter30Days
        },
        new AutoDeleteGatheringStatsFilterStruct
        {
            Name = LocalizationController.Translation("DELETE_AFTER_365_DAYS"),
            AutoDeleteGatheringStats = AutoDeleteGatheringStats.DeleteAfter365Days
        }
    };

    public AutoDeleteGatheringStats AutoDeleteStatsByDateSelection
    {
        get => _autoDeleteStatsByDateSelection;
        set
        {
            _autoDeleteStatsByDateSelection = value;
            SettingsController.CurrentSettings.AutoDeleteGatheringStats = _autoDeleteStatsByDateSelection;
            OnPropertyChanged();
        }
    }

    #endregion

    public static string TranslationGatheringActive => LocalizationController.Translation("GATHERING_ACTIVE");
}
