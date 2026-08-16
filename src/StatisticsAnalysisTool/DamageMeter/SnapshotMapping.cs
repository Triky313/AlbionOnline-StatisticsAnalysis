using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class SnapshotMapping
{
    public static DamageMeterSnapshotDto Mapping(DamageMeterSnapshot snapshot)
    {
        var allContent = snapshot.AllContent.HasData
            ? snapshot.AllContent
            : new DamageMeterContentSnapshot
            {
                DamageMeter = snapshot.DamageMeter ?? [],
                DamageStats = snapshot.DamageStats,
                YourStats = snapshot.YourStats
            };

        return new DamageMeterSnapshotDto
        {
            Timestamp = snapshot.Timestamp,
            Location = snapshot.Location,
            IsAutoSave = snapshot.IsAutoSave,
            DamageMeter = allContent.DamageMeter.Select(Mapping).ToList(),
            DamageStats = DamageStatsSnapshotFactory.Clone(allContent.DamageStats),
            YourStats = DamageMeterYourStatsSnapshotFactory.Clone(allContent.YourStats),
            ContentSnapshots = snapshot.ContentSnapshots.ToDictionary(x => x.Key, x => Mapping(x.Value))
        };
    }

    public static DamageMeterSnapshot Mapping(DamageMeterSnapshotDto snapshotDto)
    {
        var allContent = Mapping(new DamageMeterContentSnapshotDto
        {
            DamageMeter = snapshotDto.DamageMeter ?? [],
            DamageStats = snapshotDto.DamageStats,
            YourStats = snapshotDto.YourStats
        });

        var snapshot = new DamageMeterSnapshot
        {
            Timestamp = snapshotDto.Timestamp,
            Location = snapshotDto.Location,
            IsAutoSave = snapshotDto.IsAutoSave,
            AllContent = allContent,
            ContentSnapshots = (snapshotDto.ContentSnapshots ?? [])
                .ToDictionary(x => x.Key, x => Mapping(x.Value))
        };

        snapshot.ApplyContentFilter(null);
        return snapshot;
    }

    private static DamageMeterContentSnapshotDto Mapping(DamageMeterContentSnapshot snapshot)
    {
        return new DamageMeterContentSnapshotDto
        {
            DamageMeter = snapshot.DamageMeter.Select(Mapping).ToList(),
            DamageStats = DamageStatsSnapshotFactory.Clone(snapshot.DamageStats),
            YourStats = DamageMeterYourStatsSnapshotFactory.Clone(snapshot.YourStats)
        };
    }

    private static DamageMeterContentSnapshot Mapping(DamageMeterContentSnapshotDto snapshot)
    {
        var damageMeter = snapshot?.DamageMeter?.Select(Mapping).ToList() ?? [];
        return new DamageMeterContentSnapshot
        {
            DamageMeter = damageMeter,
            DamageStats = snapshot?.DamageStats ?? DamageStatsSnapshotFactory.FromSnapshotFragments(damageMeter),
            YourStats = snapshot?.YourStats ?? DamageMeterYourStatsSnapshotFactory.FromSnapshotFragments(damageMeter, null, string.Empty)
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
            TakenDamage = snapshot.TakenDamage,
            TakenDamageInPercent = snapshot.TakenDamageInPercent,
            TakenDamagePercentage = snapshot.TakenDamagePercentage,
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
            TakenDamage = snapshotFragmentDto.TakenDamage,
            TakenDamageInPercent = snapshotFragmentDto.TakenDamageInPercent,
            TakenDamagePercentage = snapshotFragmentDto.TakenDamagePercentage,
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