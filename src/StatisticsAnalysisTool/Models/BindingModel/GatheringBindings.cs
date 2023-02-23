using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class GatheringBindings : INotifyPropertyChanged
{
    private bool _isGatheringActive = true;
    private GatheringStats _gatheringStats = new();
    private ObservableRangeCollection<Gathered> _gatheredCollection = new();
    private readonly ListCollectionView _gatheredCollectionView;
    private GridLength _gridSplitterPosition = GridLength.Auto;
    private Dictionary<GatheringFilterType, string> _gatheringFilter = new()
    {
        { GatheringFilterType.Generally, LanguageController.Translation("GENERALLY") },
        { GatheringFilterType.Wood, LanguageController.Translation("WOOD") },
        { GatheringFilterType.Fiber, LanguageController.Translation("FIBER") },
        { GatheringFilterType.Hide, LanguageController.Translation("HIDE") },
        { GatheringFilterType.Ore, LanguageController.Translation("ORE") },
        { GatheringFilterType.Rock, LanguageController.Translation("ROCK") }
    };
    private GatheringFilterType _selectedGatheringFilter = GatheringFilterType.Generally;
    private GatheringStatsTimeType _gatheringStatsTimeTypeSelection = GatheringStatsTimeType.Today;

    public GatheringBindings()
    {
        IsGatheringActive = SettingsController.CurrentSettings.IsGatheringActive;

        GatheredCollectionView = CollectionViewSource.GetDefaultView(GatheredCollection) as ListCollectionView;

        if (GatheredCollectionView != null)
        {
            GatheredCollectionView.IsLiveSorting = true;
            GatheredCollectionView.IsLiveFiltering = true;
            GatheredCollectionView.CustomSort = new GatheredComparer();

            GatheredCollectionView?.Refresh();
        }

        GatheredCollection.CollectionChanged += UpdateStatsAsync;
    }

    public void UpdateStats()
    {
        UpdateStatsAsync(null, null);
    }

    public async void UpdateStatsAsync(object sender, NotifyCollectionChangedEventArgs e)
    {
        await Task.Run(async () =>
        {
            // Hide
            var hide = await GroupAndFilterAndSumAsync(GatheredCollection, x => x?.Item?.ShopShopSubCategory1 == ShopSubCategory.Hide, GatheringStatsTimeTypeSelection);
            await UpdateObservableRangeCollectionAsync(GatheringStats.GatheredHide, hide);

            GatheringStats.GainedSilverByHide = Utilities.LongWithCulture(hide.Sum(x => x.TotalMarketValue.IntegerValue));

            // Ore
            var ore = await GroupAndFilterAndSumAsync(GatheredCollection, x => x?.Item?.ShopShopSubCategory1 == ShopSubCategory.Ore, GatheringStatsTimeTypeSelection);
            await UpdateObservableRangeCollectionAsync(GatheringStats.GatheredOre, ore);

            GatheringStats.GainedSilverByOre = Utilities.LongWithCulture(ore.Sum(x => x.TotalMarketValue.IntegerValue));

            // Fiber
            var fiber = await GroupAndFilterAndSumAsync(GatheredCollection, x => x?.Item?.ShopShopSubCategory1 == ShopSubCategory.Fiber, GatheringStatsTimeTypeSelection);
            await UpdateObservableRangeCollectionAsync(GatheringStats.GatheredFiber, fiber);

            GatheringStats.GainedSilverByFiber = Utilities.LongWithCulture(fiber.Sum(x => x.TotalMarketValue.IntegerValue));

            // Wood
            var wood = await GroupAndFilterAndSumAsync(GatheredCollection, x => x?.Item?.ShopShopSubCategory1 == ShopSubCategory.Wood, GatheringStatsTimeTypeSelection);
            await UpdateObservableRangeCollectionAsync(GatheringStats.GatheredWood, wood);

            GatheringStats.GainedSilverByWood = Utilities.LongWithCulture(wood.Sum(x => x.TotalMarketValue.IntegerValue));

            // Rock
            var rock = await GroupAndFilterAndSumAsync(GatheredCollection, x => x?.Item?.ShopShopSubCategory1 == ShopSubCategory.Rock, GatheringStatsTimeTypeSelection);
            await UpdateObservableRangeCollectionAsync(GatheringStats.GatheredRock, rock);

            GatheringStats.GainedSilverByRock = Utilities.LongWithCulture(rock.Sum(x => x.TotalMarketValue.IntegerValue));

            // Most gathered resource
            var mostGatheredResource = GatheredCollection
                .Where(x => IsTimestampOkayByGatheringStatsTimeType(x.TimestampDateTime, GatheringStatsTimeTypeSelection))
                .GroupBy(x => x.UniqueName)
                .Select(g => new Gathered
                {
                    UniqueName = g.Key,
                    GainedStandardAmount = g.Sum(x => x.GainedStandardAmount),
                    GainedBonusAmount = g.Sum(x => x.GainedBonusAmount),
                    GainedPremiumBonusAmount = g.Sum(x => x.GainedPremiumBonusAmount),
                    GainedTotalAmount = g.Sum(x => x.GainedTotalAmount),
                    MiningProcesses = g.Sum(x => x.MiningProcesses)
                }).MaxBy(x => x.GainedTotalAmount);

            GatheringStats.MostGatheredResource = mostGatheredResource;

            // Most gathered cluster
            var mostGatheredCluster = GatheredCollection
                .Where(x => IsTimestampOkayByGatheringStatsTimeType(x.TimestampDateTime, GatheringStatsTimeTypeSelection))
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

            GatheringStats.MostGatheredCluster = mostGatheredCluster;

            // Most total resources
            var totalResources = GatheredCollection
                .Where(x => IsTimestampOkayByGatheringStatsTimeType(x.TimestampDateTime, GatheringStatsTimeTypeSelection))
                .Sum(x => x.GainedTotalAmount);

            GatheringStats.TotalResources = totalResources;

            // Most total mining processes
            var totalMiningProcesses = GatheredCollection
                .Where(x => IsTimestampOkayByGatheringStatsTimeType(x.TimestampDateTime, GatheringStatsTimeTypeSelection))
                .Sum(x => x.MiningProcesses);

            GatheringStats.TotalMiningProcesses = totalMiningProcesses;

            // Total gained silver
            var totalGainedSilver = GatheredCollection
                .Where(x => IsTimestampOkayByGatheringStatsTimeType(x.TimestampDateTime, GatheringStatsTimeTypeSelection))
                .Sum(x => x.TotalMarketValue.IntegerValue);

            GatheringStats.TotalGainedSilverString = Utilities.LongWithCulture(totalGainedSilver);
        });
    }

    private static async Task<List<Gathered>> GroupAndFilterAndSumAsync(IEnumerable<Gathered> gatheredData, Func<Gathered, bool> filter, GatheringStatsTimeType gatheringStatsTimeType)
    {
        return await Task.Run(() =>
        {
            var filteredData = gatheredData?.ToList().Where(filter).Where(x => IsTimestampOkayByGatheringStatsTimeType(x.TimestampDateTime, gatheringStatsTimeType));
            var groupedData = filteredData?.ToList().GroupBy(x => x.UniqueName)
                .Select(g => new Gathered()
                {
                    UniqueName = g.Key,
                    EstimatedMarketValue = FixPoint.FromInternalValue(g.FirstOrDefault()?.EstimatedMarketValue.InternalValue ?? 0),
                    GainedStandardAmount = g.Sum(x => x.GainedStandardAmount),
                    GainedBonusAmount = g.Sum(x => x.GainedBonusAmount),
                    GainedPremiumBonusAmount = g.Sum(x => x.GainedPremiumBonusAmount),
                    GainedTotalAmount = g.Sum(x => x.GainedTotalAmount), 
                    GainedFame = g.Sum(x => x.GainedFame),
                    MiningProcesses = g.Sum(x => x.MiningProcesses)
                }).ToList();

            return groupedData;
        }) ?? new List<Gathered>();
    }

    private static async Task UpdateObservableRangeCollectionAsync(ObservableRangeCollection<Gathered> target, IEnumerable<Gathered> source)
    {
        var sourceSorted = source.OrderByDescending(x => x.UniqueName);
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            target.Clear();
            target.AddRange(sourceSorted.ToList());
        });
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
                when dateTime.Year != dateTimeUtcNow.Year => false,
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
            Name = LanguageController.Translation("TODAY"),
            GatheringStatsTimeType = GatheringStatsTimeType.Today
        },
        new GatheringStatsFilterStruct
        {
            Name = LanguageController.Translation("THIS_WEEK"),
            GatheringStatsTimeType = GatheringStatsTimeType.ThisWeek
        },
        new GatheringStatsFilterStruct
        {
            Name = LanguageController.Translation("LAST_WEEK"),
            GatheringStatsTimeType = GatheringStatsTimeType.LastWeek
        },
        new GatheringStatsFilterStruct
        {
            Name = LanguageController.Translation("LAST_30_DAYS"),
            GatheringStatsTimeType = GatheringStatsTimeType.Month
        },
        new GatheringStatsFilterStruct
        {
            Name = LanguageController.Translation("LAST_365_DAYS"),
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

    #endregion

    public static string TranslationGatheringActive => LanguageController.Translation("GATHERING_ACTIVE");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}