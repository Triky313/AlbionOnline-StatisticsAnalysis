using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Media.Imaging;
using StatisticsAnalysisTool.Localization;

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
    private HealthChangeType _healthChangeType = HealthChangeType.Damage;
    private string _localizationName;
    private string _localizationDescription;

    public int SpellIndex { get; set; }

    public string UniqueName
    {
        get => _uniqueName;
        set
        {
            _uniqueName = value;
            LocalizationName = UniqueName == "AUTO_ATTACK" ? LanguageController.Translation("AUTO_ATTACK") : SpellData.GetLocalizationName(_uniqueName);
            LocalizationDescription = SpellData.GetLocalizationDescription(_uniqueName);
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

    public HealthChangeType HealthChangeType
    {
        get => _healthChangeType;
        set
        {
            _healthChangeType = value;
            OnPropertyChanged();
        }
    }

    public string LocalizationName
    {
        get => _localizationName;
        set
        {
            _localizationName = value;
            OnPropertyChanged();
        }
    }

    public string LocalizationDescription
    {
        get => _localizationDescription;
        set
        {
            _localizationDescription = value;
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