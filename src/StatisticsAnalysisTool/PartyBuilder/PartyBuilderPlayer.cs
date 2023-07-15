using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.PartyBuilder;

public class PartyBuilderPlayer : INotifyPropertyChanged
{
    private Item _mainHand;
    private Item _offHand;
    private Item _head;
    private Item _chest;
    private Item _shoes;
    private Item _bag;
    private Item _cape;
    private Item _mount;
    private Item _potion;
    private Item _buffFood;
    private string _username;
    private IEnumerable<Spell> _mainHandSpells;
    private IEnumerable<Spell> _offHandSpells;
    private IEnumerable<Spell> _headSpells;
    private IEnumerable<Spell> _chestSpells;
    private IEnumerable<Spell> _shoesSpells;
    private IEnumerable<Spell> _mountSpells;
    private IEnumerable<Spell> _potionSpells;
    private IEnumerable<Spell> _foodSpells;
    private int _averageBasicItemPower;
    private int _averageTotalItemPower;
    public Guid Guid { get; init; }

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged();
        }
    }

    public int AverageTotalItemPower
    {
        get => _averageTotalItemPower;
        set
        {
            _averageTotalItemPower = value;
            OnPropertyChanged();
        }
    }

    public int AverageBasicItemPower
    {
        get => _averageBasicItemPower;
        set
        {
            _averageBasicItemPower = value;
            OnPropertyChanged();
        }
    }

    public Item MainHand
    {
        get => _mainHand;
        set
        {
            _mainHand = value;
            AverageBasicItemPower = ItemController.GetAverageItemPower(new [] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item OffHand
    {
        get => _offHand;
        set
        {
            _offHand = value;
            AverageBasicItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Head
    {
        get => _head;
        set
        {
            _head = value;
            AverageBasicItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Chest
    {
        get => _chest;
        set
        {
            _chest = value;
            AverageBasicItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Shoes
    {
        get => _shoes;
        set
        {
            _shoes = value;
            AverageBasicItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Bag
    {
        get => _bag;
        set
        {
            _bag = value;
            OnPropertyChanged();
        }
    }

    public Item Cape
    {
        get => _cape;
        set
        {
            _cape = value;
            AverageBasicItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Mount
    {
        get => _mount;
        set
        {
            _mount = value;
            OnPropertyChanged();
        }
    }

    public Item Potion
    {
        get => _potion;
        set
        {
            _potion = value;
            OnPropertyChanged();
        }
    }

    public Item BuffFood
    {
        get => _buffFood;
        set
        {
            _buffFood = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> MainHandSpells
    {
        get => _mainHandSpells;
        set
        {
            _mainHandSpells = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> OffHandSpells
    {
        get => _offHandSpells;
        set
        {
            _offHandSpells = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> HeadSpells
    {
        get => _headSpells;
        set
        {
            _headSpells = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> ChestSpells
    {
        get => _chestSpells;
        set
        {
            _chestSpells = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> ShoesSpells
    {
        get => _shoesSpells;
        set
        {
            _shoesSpells = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> MountSpells
    {
        get => _mountSpells;
        set
        {
            _mountSpells = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> PotionSpells
    {
        get => _potionSpells;
        set
        {
            _potionSpells = value;
            OnPropertyChanged();
        }
    }

    public IEnumerable<Spell> FoodSpells
    {
        get => _foodSpells;
        set
        {
            _foodSpells = value;
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