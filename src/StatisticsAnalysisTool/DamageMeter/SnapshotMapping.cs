using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class SnapshotMapping
{
    public static DamageMeterSnapshotDto Mapping(DamageMeterSnapshot snapshot)
    {
        return new DamageMeterSnapshotDto()
        {
            Timestamp = snapshot.Timestamp,
            DamageMeter = snapshot.DamageMeter.Select(Mapping).ToList()
        };
    }

    public static DamageMeterSnapshot Mapping(DamageMeterSnapshotDto snapshotDto)
    {
        return new DamageMeterSnapshot()
        {
            Timestamp = snapshotDto.Timestamp,
            DamageMeter = snapshotDto.DamageMeter.Select(Mapping).ToList()
        };
    }

    private static DamageMeterSnapshotFragmentDto Mapping(DamageMeterSnapshotFragment snapshot)
    {
        return new DamageMeterSnapshotFragmentDto()
        {
            Name = snapshot.Name,
            CauserGuid = snapshot.CauserGuid,
            CombatTime = snapshot.CombatTime,
            Damage = snapshot.Damage,
            DamageShortString = snapshot.DamageShortString,
            Dps = snapshot.Dps,
            DpsString = snapshot.DpsString,
            DamageInPercent = snapshot.DamageInPercent,
            DamagePercentage = snapshot.DamagePercentage,
            Heal = snapshot.Heal,
            HealShortString = snapshot.HealShortString,
            HpsString = snapshot.HpsString,
            Hps = snapshot.Hps,
            HealInPercent = snapshot.HealInPercent,
            HealPercentage = snapshot.HealPercentage,
            OverhealedPercentageOfTotalHealing = snapshot.OverhealedPercentageOfTotalHealing,
            Spells = snapshot.Spells.Select(Mapping).ToList(),
            CauserMainHandItemUniqueName = snapshot.CauserMainHandItemUniqueName,
            ShopSubCategory = snapshot.ShopSubCategory
        };
    }

    private static DamageMeterSnapshotFragment Mapping(DamageMeterSnapshotFragmentDto snapshotFragmentDto)
    {
        return new DamageMeterSnapshotFragment()
        {
            Name = snapshotFragmentDto.Name,
            CauserGuid = snapshotFragmentDto.CauserGuid,
            CombatTime = snapshotFragmentDto.CombatTime,
            Damage = snapshotFragmentDto.Damage,
            DamageShortString = snapshotFragmentDto.DamageShortString,
            Dps = snapshotFragmentDto.Dps,
            DpsString = snapshotFragmentDto.DpsString,
            DamageInPercent = snapshotFragmentDto.DamageInPercent,
            DamagePercentage = snapshotFragmentDto.DamagePercentage,
            Heal = snapshotFragmentDto.Heal,
            Hps = snapshotFragmentDto.Hps,
            HealInPercent = snapshotFragmentDto.HealInPercent,
            HealPercentage = snapshotFragmentDto.HealPercentage,
            OverhealedPercentageOfTotalHealing = snapshotFragmentDto.OverhealedPercentageOfTotalHealing,
            Spells = snapshotFragmentDto.Spells.Select(Mapping).ToList(),
            CauserMainHandItemUniqueName = snapshotFragmentDto.CauserMainHandItemUniqueName,
            ShopSubCategory = snapshotFragmentDto.ShopSubCategory
        };
    }

    private static SpellFragmentDto Mapping(SpellsSnapshotFragment snapshot)
    {
        return new SpellFragmentDto()
        {
            Index = snapshot.Index,
            UniqueName = snapshot.UniqueName,
            DamageHealValue = snapshot.DamageHealValue,
            DamageHealShortString = snapshot.DamageHealShortString
        };
    }

    private static SpellsSnapshotFragment Mapping(SpellFragmentDto snapshotDto)
    {
        return new SpellsSnapshotFragment()
        {
            Index = snapshotDto.Index,
            UniqueName = snapshotDto.UniqueName,
            DamageHealValue = snapshotDto.DamageHealValue,
            DamageHealShortString = snapshotDto.DamageHealShortString
        };
    }
}