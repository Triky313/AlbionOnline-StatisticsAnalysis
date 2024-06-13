using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.DamageMeter;

public class UsedSpellFragment : BaseViewModel
{
    private string _uniqueName;
    private long _damageHealValue;
    private BitmapImage _icon;
    private string _damageHealShortString;
    private string _target;
    private string _category;
    private Item _item;
    private int _ticks;
    private int _itemIndex;
    private double _damageInPercent;
    private double _damagePercentage;

    public int SpellIndex { get; set; }

    public string UniqueName
    {
        get => _uniqueName;
        set
        {
            _uniqueName = value;
            OnPropertyChanged();
        }
    }

    public int ItemIndex
    {
        get => _itemIndex;
        set
        {
            _itemIndex = value;
            Item = ItemController.GetItemByIndex(ItemIndex);
            OnPropertyChanged();
        }
    }

    public long DamageHealValue
    {
        get => _damageHealValue;
        set
        {
            _damageHealValue = value;
            DamageHealShortString = _damageHealValue.ToShortNumberString();
            OnPropertyChanged();
        }
    }

    public string Target
    {
        get => _target;
        set
        {
            _target = value;
            OnPropertyChanged();
        }
    }

    public string Category
    {
        get => _category;
        set
        {
            _category = value;
            OnPropertyChanged();
        }
    }

    public string DamageHealShortString
    {
        get => _damageHealShortString;
        private set
        {
            _damageHealShortString = value;
            OnPropertyChanged();
        }
    }

    public int Ticks
    {
        get => _ticks;
        set
        {
            _ticks = value;
            OnPropertyChanged();
        }
    }

    public double DamageInPercent
    {
        get => _damageInPercent;
        set
        {
            _damageInPercent = value;
            OnPropertyChanged();
        }
    }

    public double DamagePercentage
    {
        get => _damagePercentage;
        set
        {
            _damagePercentage = value;
            OnPropertyChanged();
        }
    }

    public Item Item
    {
        get => _item;
        set
        {
            _item = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetSpellImage(UniqueName));
}