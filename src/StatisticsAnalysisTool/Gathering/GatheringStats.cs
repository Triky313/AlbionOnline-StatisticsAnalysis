using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringStats : INotifyPropertyChanged
{
    private ItemTier _tier = ItemTier.Unknown;
    private GatheringFilterType _gatheringFilterType = GatheringFilterType.Unknown;
    private ObservableRangeCollection<Gathered> _gatheredWood = new ();
    private ObservableRangeCollection<Gathered> _gatheredHide = new();
    private ObservableRangeCollection<Gathered> _gatheredOre = new();
    private ObservableRangeCollection<Gathered> _gatheredRock = new();
    private ObservableRangeCollection<Gathered> _gatheredFiber = new();

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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}