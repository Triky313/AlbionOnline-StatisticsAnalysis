using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.ObjectModel;

namespace StatisticsAnalysisTool.StorageHistory;

public class VaultContainer : BaseViewModel
{
    private DateTime _lastUpdate;
    private Guid _guid;
    private string _name;
    private string _icon;
    private ObservableCollection<ContainerItem> _items = new();
    private double _repairCosts;
    private double _totalValue;
    private bool _isGuildContainer;

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set
        {
            _lastUpdate = value;
            OnPropertyChanged();
        }
    }

    public Guid Guid
    {
        get => _guid;
        set
        {
            _guid = value;
            OnPropertyChanged();
        }
    }

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged();
        }
    }

    public string Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<ContainerItem> Items
    {
        get => _items;
        set
        {
            _items = value;
            OnPropertyChanged();
        }
    }

    public double RepairCosts
    {
        get => _repairCosts;
        set
        {
            _repairCosts = value;
            OnPropertyChanged();
        }
    }

    public double TotalValue
    {
        get => _totalValue;
        set
        {
            _totalValue = value;
            OnPropertyChanged();
        }
    }

    public bool IsGuildContainer
    {
        get => _isGuildContainer;
        set
        {
            _isGuildContainer = value;
            OnPropertyChanged();
        }
    }
}