using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Gathering;

public class GatheringStats : INotifyPropertyChanged
{
    private ItemTier _tier = ItemTier.Unknown;
    private GatheringFilterType _gatheringFilterType = GatheringFilterType.Unknown;

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

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}