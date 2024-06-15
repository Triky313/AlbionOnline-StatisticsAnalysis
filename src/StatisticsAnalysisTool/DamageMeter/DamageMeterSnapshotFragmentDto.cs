using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;
public sealed class DamageMeterSnapshotFragmentDto
{
    public string Name { get; set; }
    public Guid CauserGuid { get; set; }
    public TimeSpan CombatTime { get; set; }

    #region Damage

    public long Damage { get; set; }
    public string DamageShortString { get; set; }
    public double Dps { get; set; }
    public string DpsString { get; set; }
    public double DamageInPercent { get; set; }
    public double DamagePercentage { get; set; }

    #endregion

    #region Heal

    public long Heal { get; set; }
    public string HealShortString { get; set; }
    public string HpsString { get; set; }
    public double Hps { get; set; }
    public double HealInPercent { get; set; }
    public double HealPercentage { get; set; }
    public double OverhealedPercentageOfTotalHealing { get; set; }

    #endregion

    #region Spells

    public List<SpellFragmentDto> Spells { get; set; } = new ();

    #endregion

    public string CauserMainHandItemUniqueName { get; set; }
    public string ShopSubCategory { get; set; }
}