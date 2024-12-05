using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Windows;

namespace StatisticsAnalysisTool.EventLogging;

public class LootedItem : BaseViewModel
{
    private int _itemIndex;
    private DateTime _utcPickupTime;
    private int _quantity;
    private string _lootedByName;
    private string _lootedFromName;
    private string _lootedFromGuild;
    private bool _isTrash;
    private LootedItemStatus _status = LootedItemStatus.Unknown;
    private Visibility _visibility = Visibility.Visible;
    private bool _isItemFromVaultLog;

    public LootedItem()
    {
        UtcPickupTime = DateTime.UtcNow;
    }

    public int ItemIndex
    {
        get => _itemIndex;
        set
        {
            _itemIndex = value;
            OnPropertyChanged();
        }
    }

    public DateTime UtcPickupTime
    {
        get => _utcPickupTime;
        set
        {
            _utcPickupTime = value;
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

    public string LootedByName
    {
        get => _lootedByName;
        set
        {
            _lootedByName = value;
            OnPropertyChanged();
        }
    }

    public string LootedFromName
    {
        get => _lootedFromName;
        set
        {
            _lootedFromName = value;
            OnPropertyChanged();
        }
    }

    public string LootedFromGuild
    {
        get => _lootedFromGuild;
        set
        {
            _lootedFromGuild = value;
            OnPropertyChanged();
        }
    }

    public bool IsTrash
    {
        get => _isTrash;
        set
        {
            _isTrash = value;
            OnPropertyChanged();
        }
    }

    public LootedItemStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public bool IsItemFromVaultLog
    {
        get => _isItemFromVaultLog;
        set
        {
            _isItemFromVaultLog = value;
            OnPropertyChanged();
        }
    }

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(ItemIndex, Quantity, LootedByName);
    }

    public bool IsLootedFromGuildEmpty => string.IsNullOrEmpty(LootedFromGuild);

    public bool IsLootedPlayerMob => LootedFromName.ToUpper().Equals("MOB");

    public Item Item => ItemController.GetItemByIndex(ItemIndex);

    public string Tooltip => $"{Item.LocalizedName} - {Item.UniqueName}\n\n{Quantity}x Looted from {LootedFromName} at {UtcPickupTime:dddd, MMMM d, yyyy h:mm tt}";
}