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
        if (snapshot is null)
        {
            return new DamageMeterSnapshotFragmentDto();
        }

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
        if (snapshotFragmentDto is null)
        {
            return new DamageMeterSnapshotFragment();
        }

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
        if (snapshot is null)
        {
            return new SpellFragmentDto();
        }

        return new SpellFragmentDto()
        {
            SpellIndex = snapshot.SpellIndex,
            ItemIndex = snapshot.ItemIndex,
            UniqueName = snapshot.UniqueName,
            DamageHealValue = snapshot.DamageHealValue,
            DamageHealShortString = snapshot.DamageHealShortString,
            Target = snapshot.Target,
            Category = snapshot.Category,
            Ticks = snapshot.Ticks,
            HealthChangeType = snapshot.HealthChangeType
        };
    }

    private static SpellsSnapshotFragment Mapping(SpellFragmentDto snapshotDto)
    {
        if (snapshotDto is null)
        {
            return new SpellsSnapshotFragment();
        }

        return new SpellsSnapshotFragment()
        {
            SpellIndex = snapshotDto.SpellIndex,
            ItemIndex = snapshotDto.ItemIndex,
            UniqueName = snapshotDto.UniqueName,
            DamageHealValue = snapshotDto.DamageHealValue,
            DamageHealShortString = snapshotDto.DamageHealShortString,
            Target = snapshotDto.Target,
            Category = snapshotDto.Category,
            Ticks = snapshotDto.Ticks,
            HealthChangeType = snapshotDto.HealthChangeType
        };
    }
}