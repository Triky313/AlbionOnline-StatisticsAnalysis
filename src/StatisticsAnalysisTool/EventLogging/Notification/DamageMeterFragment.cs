using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace StatisticsAnalysisTool.EventLogging.Notification;

public class DamageMeterFragment : INotifyPropertyChanged
{
    private string _shopSubCategory;
    private Guid _causerGuid;
    private Item _causerMainHand;
    private long _damage;
    private double _damageInPercent;
    private double _damagePercentage;
    private double _dps;
    private string _dpsString;
    private string _name;
    private long _heal;
    private string _hpsString;
    private double _hps;
    private double _healInPercent;
    private double _healPercentage;
    private bool _isDamageMeterShowing = true;
    private string _damageShortString;
    private string _healShortString;
    private TimeSpan _combatTime;
    private long _healAndOverhealed;
    private string _healAndOverhealedShortString;
    private double _healAndOverhealedInPercent;
    private double _healAndOverhealedPercentage;
    private ProgressBar _healAndOverhealedProgressBar;

    public DamageMeterFragment(DamageMeterFragment damageMeterFragment)
    {
        CauserGuid = damageMeterFragment.CauserGuid;
        Damage = damageMeterFragment.Damage;
        Dps = damageMeterFragment.Dps;
        DamageInPercent = damageMeterFragment.DamageInPercent;
        DamagePercentage = damageMeterFragment.DamagePercentage;
        Heal = damageMeterFragment.Heal;
        Hps = damageMeterFragment.Hps;
        HealInPercent = damageMeterFragment.HealInPercent;
        HealPercentage = damageMeterFragment.HealPercentage;
        Name = damageMeterFragment.Name;
        CauserMainHand = damageMeterFragment.CauserMainHand;
    }

    public DamageMeterFragment()
    {
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

    public Guid CauserGuid
    {
        get => _causerGuid;
        set
        {
            _causerGuid = value;
            OnPropertyChanged();
        }
    }

    public bool IsDamageMeterShowing
    {
        get => _isDamageMeterShowing;
        set
        {
            _isDamageMeterShowing = value;
            OnPropertyChanged();
        }
    }

    public TimeSpan CombatTime
    {
        get => _combatTime;
        set
        {
            _combatTime = value;
            OnPropertyChanged();
        }
    }

    #region Damage

    public long Damage
    {
        get => _damage;
        set
        {
            _damage = value;
            DamageShortString = _damage.ToShortNumberString();
            OnPropertyChanged();
        }
    }

    public string DamageShortString
    {
        get => _damageShortString;
        private set
        {
            _damageShortString = value;
            OnPropertyChanged();
        }
    }

    public string DpsString
    {
        get => _dpsString;
        private set
        {
            _dpsString = value;
            OnPropertyChanged();
        }
    }

    public double Dps
    {
        get => _dps;
        set
        {
            _dps = value;
            DpsString = _dps.ToShortNumberString();
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

    #endregion

    #region Heal

    public long Heal
    {
        get => _heal;
        set
        {
            _heal = value;
            HealShortString = _heal.ToShortNumberString();
            OnPropertyChanged();
        }
    }

    public string HealShortString
    {
        get => _healShortString;
        private set
        {
            _healShortString = value;
            OnPropertyChanged();
        }
    }

    public string HpsString
    {
        get => _hpsString;
        private set
        {
            _hpsString = value;
            OnPropertyChanged();
        }
    }

    public double Hps
    {
        get => _hps;
        set
        {
            _hps = value;
            HpsString = _hps.ToShortNumberString();
            OnPropertyChanged();
        }
    }

    public double HealInPercent
    {
        get => _healInPercent;
        set
        {
            _healInPercent = value;
            OnPropertyChanged();
        }
    }

    public double HealPercentage
    {
        get => _healPercentage;
        set
        {
            _healPercentage = value;
            OnPropertyChanged();
        }
    }

    public long HealAndOverhealed
    {
        get => _healAndOverhealed;
        set
        {
            _healAndOverhealed = value;
            HealAndOverhealedShortString = _healAndOverhealed.ToShortNumberString();
            OnPropertyChanged();
        }
    }

    public string HealAndOverhealedShortString
    {
        get => _healAndOverhealedShortString;
        private set
        {
            _healAndOverhealedShortString = value;
            OnPropertyChanged();
        }
    }

    public double HealAndOverhealedInPercent
    {
        get => _healAndOverhealedInPercent;
        set
        {
            _healAndOverhealedInPercent = value;
            OnPropertyChanged();
        }
    }

    public double HealAndOverhealedPercentage
    {
        get => _healAndOverhealedPercentage;
        set
        {
            _healAndOverhealedPercentage = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public Item CauserMainHand
    {
        get => _causerMainHand;
        set
        {
            _causerMainHand = value;
            ShopSubCategory = CategoryController.ShopSubCategoryToShopSubCategoryString(_causerMainHand?.ShopShopSubCategory1 ?? Common.ShopSubCategory.Unknown);
            OnPropertyChanged();
        }
    }

    public string ShopSubCategory
    {
        get => _shopSubCategory;
        set
        {
            _shopSubCategory = value;
            OnPropertyChanged();
        }
    }

    public string TranslationCombatTime => LanguageController.Translation("COMBAT_TIME");
    public string TranslationHeal => LanguageController.Translation("HEAL");
    public string TranslationHealWithOverhealed => LanguageController.Translation("HEAL_WITH_OVERHEALED");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override bool Equals(object obj)
    {
        return obj is DamageMeterFragment damageMeterFragment && Name == damageMeterFragment.Name
                                                              && Damage == damageMeterFragment.Damage
                                                              && CauserGuid == damageMeterFragment.CauserGuid
                                                              && Heal == damageMeterFragment.Heal;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, CauserGuid, Damage, Heal);
    }
}