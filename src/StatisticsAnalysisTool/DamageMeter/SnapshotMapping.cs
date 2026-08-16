using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using System.Collections.ObjectModel;
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
                YourStats = snapshot.YourStats,
                MobDamageMeter = snapshot.MobDamageMeter ?? []
            };

        return new DamageMeterSnapshotDto
        {
            Timestamp = snapshot.Timestamp,
            Location = snapshot.Location,
            IsAutoSave = snapshot.IsAutoSave,
            DamageMeter = allContent.DamageMeter.Select(Mapping).ToList(),
            DamageStats = DamageStatsSnapshotFactory.Clone(allContent.DamageStats),
            YourStats = DamageMeterYourStatsSnapshotFactory.Clone(allContent.YourStats),
            MobDamageMeter = allContent.MobDamageMeter.Select(Mapping).ToList(),
            ContentSnapshots = snapshot.ContentSnapshots.ToDictionary(x => x.Key, x => Mapping(x.Value))
        };
    }

    public static DamageMeterSnapshot Mapping(DamageMeterSnapshotDto snapshotDto)
    {
        var allContent = Mapping(new DamageMeterContentSnapshotDto
        {
            DamageMeter = snapshotDto.DamageMeter ?? [],
            DamageStats = snapshotDto.DamageStats,
            MobDamageMeter = snapshotDto.MobDamageMeter ?? [],
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
            MobDamageMeter = snapshot.MobDamageMeter.Select(Mapping).ToList(),
            YourStats = DamageMeterYourStatsSnapshotFactory.Clone(snapshot.YourStats)
        };
    }

    private static DamageMeterContentSnapshot Mapping(DamageMeterContentSnapshotDto snapshot)
    {
        var damageMeter = snapshot?.DamageMeter?.Select(Mapping).ToList() ?? [];
        return new DamageMeterContentSnapshot
        {
            DamageMeter = damageMeter,
            MobDamageMeter = snapshot?.MobDamageMeter?.Select(Mapping).ToList() ?? [],
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

    private static MobDamageMeterFragmentDto Mapping(MobDamageMeterFragment fragment)
    {
        if (fragment is null)
        {
            return new MobDamageMeterFragmentDto();
        }

        return new MobDamageMeterFragmentDto
        {
            MobInstanceId = fragment.MobInstanceId,
            MobObjectId = fragment.MobObjectId,
            Name = fragment.Name,
            UniqueName = fragment.UniqueName,
            ClusterName = fragment.ClusterName,
            Damage = fragment.Damage,
            DamagePercentage = fragment.DamagePercentage,
            FirstAttackTime = fragment.FirstAttackTime,
            CombatTime = fragment.CombatTime,
            Dps = fragment.Dps,
            MobTier = (short) fragment.MobTier,
            MobType = fragment.MobType,
            MobRank = fragment.MobRank,
            ContentType = fragment.ContentType,
            MapName = fragment.MapName,
            MapTier = fragment.MapTier,
            Players = fragment.Players
                .Select(x => Mapping(new DamageMeterSnapshotFragment(x)))
                .ToList()
        };
    }

    private static MobDamageMeterFragment Mapping(MobDamageMeterFragmentDto snapshotDto)
    {
        if (snapshotDto is null)
        {
            return new MobDamageMeterFragment();
        }

        var mobData = MobsData.GetMobByUniqueNameOrDefault(snapshotDto.UniqueName ?? string.Empty);
        return new MobDamageMeterFragment
        {
            MobInstanceId = snapshotDto.MobInstanceId,
            MobObjectId = snapshotDto.MobObjectId,
            Name = snapshotDto.Name ?? string.Empty,
            UniqueName = snapshotDto.UniqueName ?? string.Empty,
            ClusterName = snapshotDto.ClusterName ?? string.Empty,
            AvatarSource = MobAvatarImageProvider.GetAvatarSource(MobsData.GetAvatarFileName(mobData)),
            Damage = snapshotDto.Damage,
            DamagePercentage = snapshotDto.DamagePercentage,
            FirstAttackTime = snapshotDto.FirstAttackTime,
            CombatTime = snapshotDto.CombatTime,
            Dps = snapshotDto.Dps,
            MobTier = (Tier) snapshotDto.MobTier,
            MobType = snapshotDto.MobType ?? string.Empty,
            ContentType = snapshotDto.ContentType,
            MapName = snapshotDto.MapName ?? string.Empty,
            MapTier = snapshotDto.MapTier,
            Players = new ObservableCollection<DamageMeterFragment>(
                (snapshotDto.Players ?? []).Select(MappingMobPlayer))
        };
    }

    private static DamageMeterFragment MappingMobPlayer(DamageMeterSnapshotFragmentDto snapshotDto)
    {
        if (snapshotDto is null)
        {
            return new DamageMeterFragment();
        }

        return new DamageMeterFragment
        {
            Name = snapshotDto.Name,
            CauserGuid = snapshotDto.CauserGuid,
            CombatTime = snapshotDto.CombatTime,
            Damage = snapshotDto.Damage,
            Dps = snapshotDto.Dps,
            DamageInPercent = snapshotDto.DamageInPercent,
            DamagePercentage = snapshotDto.DamagePercentage,
            CauserMainHand = ItemController.GetItemByUniqueName(snapshotDto.CauserMainHandItemUniqueName ?? string.Empty),
            DamageMeterStyleFragmentType = DamageMeterStyleFragmentType.Damage,
            Spells = new ObservableCollection<UsedSpellFragment>(
                (snapshotDto.Spells ?? []).Select(MappingMobSpell))
        };
    }

    private static UsedSpellFragment MappingMobSpell(SpellFragmentDto snapshotDto)
    {
        if (snapshotDto is null)
        {
            return new UsedSpellFragment();
        }

        return new UsedSpellFragment
        {
            SpellIndex = snapshotDto.SpellIndex,
            ItemIndex = snapshotDto.ItemIndex,
            UniqueName = snapshotDto.UniqueName ?? string.Empty,
            DamageHealValue = snapshotDto.DamageHealValue,
            Target = snapshotDto.Target ?? string.Empty,
            Category = snapshotDto.Category ?? string.Empty,
            Ticks = snapshotDto.Ticks,
            DamageInPercent = snapshotDto.DamageInPercent,
            DamagePercentage = snapshotDto.DamagePercentage,
            HealthChangeType = snapshotDto.HealthChangeType
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
            DamageInPercent = snapshot.DamageInPercent,
            DamagePercentage = snapshot.DamagePercentage,
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
            DamageInPercent = snapshotDto.DamageInPercent,
            DamagePercentage = snapshotDto.DamagePercentage,
            HealthChangeType = snapshotDto.HealthChangeType
        };
    }
}