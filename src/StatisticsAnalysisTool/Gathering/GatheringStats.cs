using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringStats : BaseViewModel
{
    private ItemTier _tier = ItemTier.Unknown;
    private GatheringFilterType _gatheringFilterType = GatheringFilterType.Unknown;
    private ObservableRangeCollection<Gathered> _gatheredWood = new();
    private ObservableRangeCollection<Gathered> _gatheredHide = new();
    private ObservableRangeCollection<Gathered> _gatheredOre = new();
    private ObservableRangeCollection<Gathered> _gatheredRock = new();
    private ObservableRangeCollection<Gathered> _gatheredFiber = new();
    private ObservableRangeCollection<Gathered> _gatheredFish = new();
    private Gathered _mostGatheredResource = new();
    private Gathered _mostGatheredCluster;
    private long _totalMiningProcesses;
    private long _totalResources;
    private Visibility _mostGatheredResourceVisibility = Visibility.Collapsed;
    private Visibility _mostGatheredClusterVisibility = Visibility.Collapsed;
    private long _gainedSilverByHide;
    private long _gainedSilverByOre;
    private long _gainedSilverByRock;
    private long _gainedSilverByFiber;
    private long _gainedSilverByWood;
    private long _totalGainedSilverString;
    private long _gainedSilverByFish;
    private double _gainedSilverPerHourByHide;
    private double _gainedSilverPerHourByOre;
    private double _gainedSilverPerHourByRock;
    private double _gainedSilverPerHourByFiber;
    private double _gainedSilverPerHourByWood;
    private double _gainedSilverPerHourByFish;
    private double _totalGainedSilverPerHour;
    private ObservableCollection<GatheringChartSeriesFilter> _resourceChartSeriesFilters = new();
    private ObservableCollection<ISeries> _resourceChartSeries = [];
    private Axis[] _resourceChartXAxes = [];
    private GatheringChartValueType _selectedResourceChartValueType = GatheringChartValueType.ResourceAmount;

    public GatheringStats()
    {
        ResourceChartSeriesFilters = new ObservableCollection<GatheringChartSeriesFilter>(GatheringChartSeriesFilter.CreateDefault());
    }

    public GatheringFilterType GatheringFilterType
    {
        get => _gatheringFilterType;
        set
        {
            _gatheringFilterType = value;
            OnPropertyChanged();
        }
    }

    public ItemTier Tier
    {
        get => _tier;
        set
        {
            _tier = value;
            OnPropertyChanged();
        }
    }

    public Gathered MostGatheredResource
    {
        get => _mostGatheredResource;
        set
        {
            _mostGatheredResource = value;
            MostGatheredResourceVisibility = value is { GainedTotalAmount: > 0 } ? Visibility.Visible : Visibility.Collapsed;
            OnPropertyChanged();
        }
    }

    public Visibility MostGatheredResourceVisibility
    {
        get => _mostGatheredResourceVisibility;
        set
        {
            _mostGatheredResourceVisibility = value;
            OnPropertyChanged();
        }
    }

    public Gathered MostGatheredCluster
    {
        get => _mostGatheredCluster;
        set
        {
            _mostGatheredCluster = value;
            if (value != null && !string.IsNullOrEmpty(value.ClusterIndex))
            {
                MostGatheredClusterVisibility = Visibility.Visible;
            }
            else
            {
                MostGatheredClusterVisibility = Visibility.Collapsed;
            }
            OnPropertyChanged();
        }
    }

    public Visibility MostGatheredClusterVisibility
    {
        get => _mostGatheredClusterVisibility;
        set
        {
            _mostGatheredClusterVisibility = value;
            OnPropertyChanged();
        }
    }

    public long TotalMiningProcesses
    {
        get => _totalMiningProcesses;
        set
        {
            _totalMiningProcesses = value;
            OnPropertyChanged();
        }
    }

    public long TotalResources
    {
        get => _totalResources;
        set
        {
            _totalResources = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredHide
    {
        get => _gatheredHide;
        set
        {
            _gatheredHide = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredOre
    {
        get => _gatheredOre;
        set
        {
            _gatheredOre = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredRock
    {
        get => _gatheredRock;
        set
        {
            _gatheredRock = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredFiber
    {
        get => _gatheredFiber;
        set
        {
            _gatheredFiber = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredWood
    {
        get => _gatheredWood;
        set
        {
            _gatheredWood = value;
            OnPropertyChanged();
        }
    }

    public ObservableRangeCollection<Gathered> GatheredFish
    {
        get => _gatheredFish;
        set
        {
            _gatheredFish = value;
            OnPropertyChanged();
        }
    }

    public long GainedSilverByHide
    {
        get => _gainedSilverByHide;
        set
        {
            _gainedSilverByHide = value;
            OnPropertyChanged();
        }
    }

    public long GainedSilverByOre
    {
        get => _gainedSilverByOre;
        set
        {
            _gainedSilverByOre = value;
            OnPropertyChanged();
        }
    }

    public long GainedSilverByRock
    {
        get => _gainedSilverByRock;
        set
        {
            _gainedSilverByRock = value;
            OnPropertyChanged();
        }
    }

    public long GainedSilverByFiber
    {
        get => _gainedSilverByFiber;
        set
        {
            _gainedSilverByFiber = value;
            OnPropertyChanged();
        }
    }

    public long GainedSilverByWood
    {
        get => _gainedSilverByWood;
        set
        {
            _gainedSilverByWood = value;
            OnPropertyChanged();
        }
    }

    public long GainedSilverByFish
    {
        get => _gainedSilverByFish;
        set
        {
            _gainedSilverByFish = value;
            OnPropertyChanged();
        }
    }

    public double GainedSilverPerHourByHide
    {
        get => _gainedSilverPerHourByHide;
        set
        {
            _gainedSilverPerHourByHide = value;
            OnPropertyChanged();
        }
    }

    public double GainedSilverPerHourByOre
    {
        get => _gainedSilverPerHourByOre;
        set
        {
            _gainedSilverPerHourByOre = value;
            OnPropertyChanged();
        }
    }

    public double GainedSilverPerHourByRock
    {
        get => _gainedSilverPerHourByRock;
        set
        {
            _gainedSilverPerHourByRock = value;
            OnPropertyChanged();
        }
    }

    public double GainedSilverPerHourByFiber
    {
        get => _gainedSilverPerHourByFiber;
        set
        {
            _gainedSilverPerHourByFiber = value;
            OnPropertyChanged();
        }
    }

    public double GainedSilverPerHourByWood
    {
        get => _gainedSilverPerHourByWood;
        set
        {
            _gainedSilverPerHourByWood = value;
            OnPropertyChanged();
        }
    }

    public double GainedSilverPerHourByFish
    {
        get => _gainedSilverPerHourByFish;
        set
        {
            _gainedSilverPerHourByFish = value;
            OnPropertyChanged();
        }
    }

    public double TotalGainedSilverPerHour
    {
        get => _totalGainedSilverPerHour;
        set
        {
            _totalGainedSilverPerHour = value;
            OnPropertyChanged();
        }
    }

    public long TotalGainedSilverString
    {
        get => _totalGainedSilverString;
        set
        {
            _totalGainedSilverString = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<GatheringChartSeriesFilter> ResourceChartSeriesFilters
    {
        get => _resourceChartSeriesFilters;
        set
        {
            _resourceChartSeriesFilters = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ISeries> ResourceChartSeries
    {
        get => _resourceChartSeries;
        set
        {
            _resourceChartSeries = value;
            OnPropertyChanged();
        }
    }

    public Axis[] ResourceChartXAxes
    {
        get => _resourceChartXAxes;
        set
        {
            _resourceChartXAxes = value;
            OnPropertyChanged();
        }
    }

    public GatheringChartValueType SelectedResourceChartValueType
    {
        get => _selectedResourceChartValueType;
        set
        {
            _selectedResourceChartValueType = value;
            OnPropertyChanged();
        }
    }

    public List<GatheringChartValueFilterStruct> ResourceChartValueTypes { get; } =
    [
        new GatheringChartValueFilterStruct
        {
            Name = LocalizationController.Translation("GATHERING_CHART_VALUE_RESOURCE_AMOUNT"),
            GatheringChartValueType = GatheringChartValueType.ResourceAmount
        },
        new GatheringChartValueFilterStruct
        {
            Name = LocalizationController.Translation("GATHERING_CHART_VALUE_RESOURCE_SILVER_VALUE"),
            GatheringChartValueType = GatheringChartValueType.ResourceSilverValue
        }
    ];

    public static string TranslationMostGatheredResource => LocalizationController.Translation("MOST_GATHERED_RESOURCE");
    public static string TranslationMostGatheredOnMap => LocalizationController.Translation("MOST_GATHERED_ON_MAP");
    public static string TranslationTotalResources => LocalizationController.Translation("TOTAL_RESOURCES");
    public static string TranslationTotalMiningProcesses => LocalizationController.Translation("TOTAL_MINING_PROCESSES");
    public static string TranslationResourceValue => LocalizationController.Translation("RESOURCE_VALUE");
    public static string TranslationPerHour => LocalizationController.Translation("PER_HOUR");
    public static string TranslationHistory => LocalizationController.Translation("HISTORY");
    public static string TranslationChartValue => LocalizationController.Translation("GATHERING_CHART_VALUE_TYPE");
}