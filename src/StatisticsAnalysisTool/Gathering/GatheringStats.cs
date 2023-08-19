using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
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
    private string _gainedSilverByHide;
    private string _gainedSilverByOre;
    private string _gainedSilverByRock;
    private string _gainedSilverByFiber;
    private string _gainedSilverByWood;
    private string _totalGainedSilverString;
    private string _gainedSilverByFish;

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

    public string GainedSilverByHide
    {
        get => _gainedSilverByHide;
        set
        {
            _gainedSilverByHide = value;
            OnPropertyChanged();
        }
    }

    public string GainedSilverByOre
    {
        get => _gainedSilverByOre;
        set
        {
            _gainedSilverByOre = value;
            OnPropertyChanged();
        }
    }

    public string GainedSilverByRock
    {
        get => _gainedSilverByRock;
        set
        {
            _gainedSilverByRock = value;
            OnPropertyChanged();
        }
    }

    public string GainedSilverByFiber
    {
        get => _gainedSilverByFiber;
        set
        {
            _gainedSilverByFiber = value;
            OnPropertyChanged();
        }
    }

    public string GainedSilverByWood
    {
        get => _gainedSilverByWood;
        set
        {
            _gainedSilverByWood = value;
            OnPropertyChanged();
        }
    }

    public string GainedSilverByFish
    {
        get => _gainedSilverByFish;
        set
        {
            _gainedSilverByFish = value;
            OnPropertyChanged();
        }
    }

    public string TotalGainedSilverString
    {
        get => _totalGainedSilverString;
        set
        {
            _totalGainedSilverString = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationMostGatheredResource => LanguageController.Translation("MOST_GATHERED_RESOURCE");
    public static string TranslationMostGatheredOnMap => LanguageController.Translation("MOST_GATHERED_ON_MAP");
    public static string TranslationTotalResources => LanguageController.Translation("TOTAL_RESOURCES");
    public static string TranslationTotalMiningProcesses => LanguageController.Translation("TOTAL_MINING_PROCESSES");
    public static string TranslationResourceValue => LanguageController.Translation("RESOURCE_VALUE");
}