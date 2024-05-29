using System;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Party;

public class PartyBuilderItemPower : BaseViewModel
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
}