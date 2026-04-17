using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using StatisticsAnalysisTool.Diagnostics;

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

            var hideEntries = FilterGatheredEntries(filteredGatherCollection, x => x?.Item?.FullItemInformation?.ShopSubCategory2 == "hide");
            var oreEntries = FilterGatheredEntries(filteredGatherCollection, x => x?.Item?.FullItemInformation?.ShopSubCategory2 == "ore");
            var fiberEntries = FilterGatheredEntries(filteredGatherCollection, x => x?.Item?.FullItemInformation?.ShopSubCategory2 == "fiber");
            var woodEntries = FilterGatheredEntries(filteredGatherCollection, x => x?.Item?.FullItemInformation?.ShopSubCategory2 == "wood");
            var rockEntries = FilterGatheredEntries(filteredGatherCollection, x => x?.Item?.FullItemInformation?.ShopSubCategory2 == "rock");
            var fishEntries = FilterGatheredEntries(filteredGatherCollection, x => x?.Item?.FullItemInformation?.ShopSubCategory2 == "fish", true);

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
        }
        catch (Exception ex)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static List<Gathered> GetGatheredEntriesByTimeFilter(IEnumerable<Gathered> gatheredData, GatheringStatsTimeType gatheringStatsTimeType)
    {
        return gatheredData
            .Where(x => IsTimestampOkayByGatheringStatsTimeType(x.TimestampDateTimeUtc, gatheringStatsTimeType))
            .ToList();
    }

    private static List<Gathered> FilterGatheredEntries(IEnumerable<Gathered> gatheredData, Func<Gathered, bool> filter, bool hasBeenFished = false)
    {
        return hasBeenFished
            ? gatheredData.Where(x => x.HasBeenFished).ToList()
            : gatheredData.Where(filter).Where(x => x.HasBeenFished == false).ToList();
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
                existingItem.GainedBonusAmount = item.GainedBonusAmount;
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
        var dateTimeUtcNow = DateTime.UtcNow;
        var result = gatheringStatsTimeType switch
        {
            GatheringStatsTimeType.Today
                when dateTime.Year != dateTimeUtcNow.Year || dateTime.Date.DayOfYear != dateTimeUtcNow.DayOfYear => false,
            GatheringStatsTimeType.ThisWeek
                when dateTime.Year != dateTimeUtcNow.Year || !dateTime.Date.IsDateInWeekOfYear(dateTimeUtcNow) => false,
            GatheringStatsTimeType.LastWeek
                when dateTime.Year != dateTimeUtcNow.Year || !dateTime.Date.IsDateInWeekOfYear(dateTimeUtcNow.AddDays(-7)) => false,
            GatheringStatsTimeType.Month
                when dateTime.Year != dateTimeUtcNow.Year || dateTime.Month != dateTimeUtcNow.Month => false,
            GatheringStatsTimeType.Year
                when dateTime < dateTimeUtcNow.AddDays(-365) || dateTime > dateTimeUtcNow => false,
            GatheringStatsTimeType.Unknown => false,
            _ => true
        };

        return result;
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