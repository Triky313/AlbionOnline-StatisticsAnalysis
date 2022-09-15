using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models;
public class DamageMeterSnapshotFragment
{
    private string _causerMainHandItemUniqueName;
    private long _damage;
    private double _dps;
    private long _heal;
    private double _hps;

    public DamageMeterSnapshotFragment(DamageMeterFragment damageMeterFragment)
    {
        Name = damageMeterFragment.Name;
        CauserGuid = damageMeterFragment.CauserGuid;
        Damage = damageMeterFragment.Damage;
        Dps = damageMeterFragment.Dps;
        DamageInPercent = damageMeterFragment.DamageInPercent;
        DamagePercentage = damageMeterFragment.DamagePercentage;
        Heal = damageMeterFragment.Heal;
        Hps = damageMeterFragment.Hps;
        HealInPercent = damageMeterFragment.HealInPercent;
        HealPercentage = damageMeterFragment.HealPercentage;
        CauserMainHandItemUniqueName = damageMeterFragment.CauserMainHand?.UniqueName ?? string.Empty;
    }

    public DamageMeterSnapshotFragment()
    {
    }

    public string Name { get; set; }
    public Guid CauserGuid { get; set; }
    public bool IsDamageMeterShowing { get; set; } = true;

    #region Damage

    public long Damage
    {
        get => _damage;
        set
        {
            _damage = value;
            DamageShortString = _damage.ToShortNumberString();
        }
    }

    public string DamageShortString { get; set; }

    public double Dps
    {
        get => _dps;
        set
        {
            _dps = value;
            DpsString = _dps.ToShortNumberString();
        }
    }

    public string DpsString { get; set; }

    public double DamageInPercent { get; set; }

    public double DamagePercentage { get; set; }

    #endregion

    #region Heal

    public long Heal
    {
        get => _heal;
        set
        {
            _heal = value;
            HealShortString = _heal.ToShortNumberString();
        }
    }

    public string HealShortString { get; private set; }

    public string HpsString { get; private set; }

    public double Hps
    {
        get => _hps;
        set
        {
            _hps = value;
            HpsString = _hps.ToShortNumberString();
        }
    }

    public double HealInPercent { get; set; }

    public double HealPercentage { get; set; }

    #endregion

    public string CauserMainHandItemUniqueName
    {
        get => _causerMainHandItemUniqueName;
        set
        {
            _causerMainHandItemUniqueName = value;
            var item = ItemController.GetItemByUniqueName(_causerMainHandItemUniqueName);
            ShopSubCategory = CategoryController.ShopSubCategoryToShopSubCategoryString(item?.ShopShopSubCategory1 ?? Common.ShopSubCategory.Unknown);
        }
    }

    [JsonIgnore]
    public Item CauserMainHand => ItemController.GetItemByUniqueName(CauserMainHandItemUniqueName);
    public string ShopSubCategory { get; set; }
}