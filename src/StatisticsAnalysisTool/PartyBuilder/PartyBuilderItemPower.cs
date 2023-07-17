using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.PartyBuilder;

public class PartyBuilderItemPower : INotifyPropertyChanged
{
    private double _itemPower;
    private bool _isItemPowerAvailable;
    private DateTime _itemPowerUpdate;

    public double ItemPower
    {
        get => _itemPower;
        set
        {
            _itemPower = value;
            ItemPowerUpdate = DateTime.Now;
            IsItemPowerAvailable = value > 0;
            OnPropertyChanged();
        }
    }

    public bool IsItemPowerAvailable
    {
        get => _isItemPowerAvailable;
        private set
        {
            _isItemPowerAvailable = value;
            OnPropertyChanged();
        }
    }

    public DateTime ItemPowerUpdate
    {
        get => _itemPowerUpdate;
        private set
        {
            _itemPowerUpdate = value;
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