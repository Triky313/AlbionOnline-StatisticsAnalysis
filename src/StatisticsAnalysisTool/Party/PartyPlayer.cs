using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Party;

public class PartyPlayer : BaseViewModel
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
    private PartyBuilderItemPower _averageBasicItemPower = new();
    private PartyBuilderItemPower _averageItemPower = new();
    private bool _isLocalPlayer;
    private PartyBuilderItemPowerCondition _basicItemPowerCondition;
    private PartyBuilderItemPowerCondition _itemPowerCondition;
    private bool _isPlayerInspected;
    private DateTime _lastUpdate;
    private bool _isDeathAlertActive;
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

    public bool IsLocalPlayer
    {
        get => _isLocalPlayer;
        set
        {
            _isLocalPlayer = value;
            if (_isLocalPlayer)
            {
                IsPlayerInspected = true;
            }
            OnPropertyChanged();
        }
    }

    public bool IsPlayerInspected
    {
        get => _isPlayerInspected;
        set
        {
            _isPlayerInspected = IsLocalPlayer || value;
            OnPropertyChanged();
        }
    }

    public PartyBuilderItemPower AverageItemPower
    {
        get => _averageItemPower;
        set
        {
            _averageItemPower = value;
            LastUpdate = DateTime.UtcNow;
            OnPropertyChanged();
        }
    }

    public PartyBuilderItemPower AverageBasicItemPower
    {
        get => _averageBasicItemPower;
        set
        {
            _averageBasicItemPower = value;
            IsPlayerInspected = false;
            LastUpdate = DateTime.UtcNow;
            OnPropertyChanged();
        }
    }

    public PartyBuilderItemPowerCondition BasicItemPowerCondition
    {
        get => _basicItemPowerCondition;
        set
        {
            _basicItemPowerCondition = value;
            OnPropertyChanged();
        }
    }

    public PartyBuilderItemPowerCondition ItemPowerCondition
    {
        get => _itemPowerCondition;
        set
        {
            _itemPowerCondition = value;
            OnPropertyChanged();
        }
    }

    public Item MainHand
    {
        get => _mainHand;
        set
        {
            _mainHand = value;
            AverageBasicItemPower.ItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item OffHand
    {
        get => _offHand;
        set
        {
            _offHand = value;
            AverageBasicItemPower.ItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Head
    {
        get => _head;
        set
        {
            _head = value;
            AverageBasicItemPower.ItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Chest
    {
        get => _chest;
        set
        {
            _chest = value;
            AverageBasicItemPower.ItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Shoes
    {
        get => _shoes;
        set
        {
            _shoes = value;
            AverageBasicItemPower.ItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Bag
    {
        get => _bag;
        set
        {
            _bag = value;
            LastUpdate = DateTime.UtcNow;
            OnPropertyChanged();
        }
    }

    public Item Cape
    {
        get => _cape;
        set
        {
            _cape = value;
            AverageBasicItemPower.ItemPower = ItemController.GetAverageItemPower(new[] { MainHand, OffHand, Head, Chest, Shoes, Cape });
            OnPropertyChanged();
        }
    }

    public Item Mount
    {
        get => _mount;
        set
        {
            _mount = value;
            LastUpdate = DateTime.UtcNow;
            OnPropertyChanged();
        }
    }

    public Item Potion
    {
        get => _potion;
        set
        {
            _potion = value;
            LastUpdate = DateTime.UtcNow;
            OnPropertyChanged();
        }
    }

    public Item BuffFood
    {
        get => _buffFood;
        set
        {
            _buffFood = value;
            LastUpdate = DateTime.UtcNow;
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

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set
        {
            _lastUpdate = value;
            OnPropertyChanged();
        }
    }

    public bool IsDeathAlertActive
    {
        get => _isDeathAlertActive;
        set
        {
            _isDeathAlertActive = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationItemPower => LanguageController.Translation("ITEM_POWER");
    public static string TranslationLastUpdate => LanguageController.Translation("LAST_UPDATE");
    public static string TranslationSelectForDeathAlert => LanguageController.Translation("SELECT_FOR_DEATH_ALERT");
}