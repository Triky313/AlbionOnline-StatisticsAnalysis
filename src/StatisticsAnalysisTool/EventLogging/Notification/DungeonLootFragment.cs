using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Network.Notification;

public class DungeonLootFragment : INotifyPropertyChanged
{
    private string _uniqueName;
    private DateTime _utcDiscoveryTime;
    private int _quantity;
    private FixPoint _estimatedMarketValue;

    public string UniqueName
    {
        get => _uniqueName;
        set
        {
            _uniqueName = value;
            OnPropertyChanged();
        }
    }

    public DateTime UtcDiscoveryTime
    {
        get => _utcDiscoveryTime;
        set
        {
            _utcDiscoveryTime = value;
            OnPropertyChanged();
        }
    }

    public int Quantity
    {
        get => _quantity;
        set
        {
            _quantity = value;
            OnPropertyChanged();
        }
    }

    public FixPoint EstimatedMarketValue
    {
        get => _estimatedMarketValue;
        set
        {
            _estimatedMarketValue = value;
            OnPropertyChanged();
        }
    }

    public FixPoint TotalEstimatedMarketValue => Quantity * EstimatedMarketValue;

    public string Hash => $"{UniqueName}{UtcDiscoveryTime.Ticks}{Quantity}{EstimatedMarketValue.InternalValue}";

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}