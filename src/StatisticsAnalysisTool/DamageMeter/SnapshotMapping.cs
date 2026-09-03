using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using System;
using System.Collections.Generic;
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
        var mobDamageMeterPlayers = new List<MobDamageMeterPlayerIdentityDto>();
        var mobDamageMeterPlayerIds = new Dictionary<string, int>(StringComparer.Ordinal);

        return new DamageMeterSnapshotDto
        {
            Timestamp = snapshot.Timestamp,
            Location = snapshot.Location,
            IsAutoSave = snapshot.IsAutoSave,
            DamageMeter = allContent.DamageMeter.Select(Mapping).ToList(),
            DamageStats = DamageStatsSnapshotFactory.Clone(allContent.DamageStats),
            YourStats = DamageMeterYourStatsSnapshotFactory.Clone(allContent.YourStats),
            MobDamageMeter = allContent.MobDamageMeter
                .Select(x => Mapping(x, mobDamageMeterPlayers, mobDamageMeterPlayerIds))
                .ToList(),
            MobDamageMeterPlayers = mobDamageMeterPlayers,
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
        allContent.MobDamageMeter = (snapshotDto.MobDamageMeter ?? [])
            .Select(x => Mapping(x, snapshotDto.MobDamageMeterPlayers ?? []))
            .ToList();

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
            DamageStats = DamageStatsSnapshotFactory.WithSnapshotFragmentFallback(snapshot?.DamageStats, damageMeter),
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

    private static MobDamageMeterFragmentDto Mapping(
        MobDamageMeterFragment fragment,
        ICollection<MobDamageMeterPlayerIdentityDto> players,
        IDictionary<string, int> playerIds)
    {
        if (fragment is null)
        {
            return new MobDamageMeterFragmentDto();
        }

        var mobData = MobsData.GetMobByUniqueNameOrDefault(fragment.UniqueName);
        return new MobDamageMeterFragmentDto
        {
            MobInstanceId = fragment.MobInstanceId,
            Name = string.IsNullOrWhiteSpace(mobData.UniqueName) ? fragment.Name : null,
            UniqueName = fragment.UniqueName,
            Damage = fragment.Damage,
            FirstAttackTime = fragment.FirstAttackTime,
            CombatTime = fragment.CombatTime,
            ContentType = fragment.ContentType,
            MapName = fragment.MapName,
            MapTier = fragment.MapTier,
            Players = fragment.Players
                .Select(x => Mapping(x, players, playerIds))
                .ToList()
        };
    }

    private static MobDamageMeterPlayerDto Mapping(
        DamageMeterFragment fragment,
        ICollection<MobDamageMeterPlayerIdentityDto> players,
        IDictionary<string, int> playerIds)
    {
        return new MobDamageMeterPlayerDto
        {
            PlayerId = GetOrAddPlayerId(fragment, players, playerIds),
            CombatTime = fragment.CombatTime,
            Damage = fragment.Damage,
            CauserMainHandItemUniqueName = fragment.CauserMainHand?.UniqueName ?? string.Empty,
            Spells = (fragment.Spells ?? [])
                .Select(Mapping)
                .ToList()
        };
    }

    private static int GetOrAddPlayerId(
        DamageMeterFragment fragment,
        ICollection<MobDamageMeterPlayerIdentityDto> players,
        IDictionary<string, int> playerIds)
    {
        var identityKey = fragment.CauserGuid != Guid.Empty
            ? $"G:{fragment.CauserGuid:D}"
            : $"N:{fragment.Name?.Trim().ToUpperInvariant() ?? string.Empty}";
        if (playerIds.TryGetValue(identityKey, out var playerId))
        {
            return playerId;
        }

        players.Add(new MobDamageMeterPlayerIdentityDto
        {
            CauserGuid = fragment.CauserGuid,
            Name = fragment.Name ?? string.Empty
        });
        playerId = players.Count;
        playerIds.Add(identityKey, playerId);
        return playerId;
    }

    private static MobDamageMeterSpellDto Mapping(UsedSpellFragment fragment)
    {
        var spellData = SpellData.GetSpellByIndex(fragment.SpellIndex);
        var resolvedUniqueName = fragment.SpellIndex <= 0 ? "AUTO_ATTACK" : spellData.UniqueName;
        return new MobDamageMeterSpellDto
        {
            SpellIndex = fragment.SpellIndex,
            ItemUniqueName = fragment.Item?.UniqueName,
            DamageHealValue = fragment.DamageHealValue,
            Ticks = fragment.Ticks,
            UniqueName = string.Equals(fragment.UniqueName, resolvedUniqueName, StringComparison.Ordinal)
                ? null
                : fragment.UniqueName,
            Target = string.Equals(fragment.Target, spellData.Target, StringComparison.Ordinal)
                ? null
                : fragment.Target,
            Category = string.Equals(fragment.Category, spellData.Category, StringComparison.Ordinal)
                ? null
                : fragment.Category
        };
    }

    private static MobDamageMeterFragment Mapping(
        MobDamageMeterFragmentDto snapshotDto,
        IReadOnlyList<MobDamageMeterPlayerIdentityDto> players)
    {
        if (snapshotDto is null)
        {
            return new MobDamageMeterFragment();
        }

        var uniqueName = snapshotDto.UniqueName ?? string.Empty;
        var mobData = MobsData.GetMobByUniqueNameOrDefault(uniqueName);
        var playerDtos = snapshotDto.Players ?? [];
        var maximumPlayerDamage = playerDtos.Count > 0 ? playerDtos.Max(x => x.Damage) : 0;
        var totalPlayerDamage = playerDtos.Sum(x => x.Damage);
        return new MobDamageMeterFragment
        {
            MobInstanceId = snapshotDto.MobInstanceId,
            MobObjectId = snapshotDto.MobObjectId,
            Name = ResolveMobName(mobData, uniqueName, snapshotDto.Name, snapshotDto.MobObjectId),
            UniqueName = uniqueName,
            ClusterName = snapshotDto.ClusterName ?? snapshotDto.MapName ?? string.Empty,
            AvatarSource = MobAvatarImageProvider.GetAvatarSource(MobsData.GetAvatarFileName(mobData)),
            Damage = snapshotDto.Damage,
            FirstAttackTime = snapshotDto.FirstAttackTime,
            CombatTime = snapshotDto.CombatTime,
            Dps = CalculateDps(snapshotDto.Damage, snapshotDto.CombatTime),
            MobTier = (Tier) mobData.Tier,
            MobType = ResolveMobType(mobData),
            MobTypeCategory = mobData.MobTypeCategory ?? string.Empty,
            Category = mobData.Category ?? string.Empty,
            Faction = mobData.Faction ?? string.Empty,
            AttackType = mobData.AttackType ?? string.Empty,
            ContentType = snapshotDto.ContentType,
            MapName = snapshotDto.MapName ?? string.Empty,
            MapTier = snapshotDto.MapTier,
            Players = new ObservableCollection<DamageMeterFragment>(
                playerDtos.Select(x => MappingMobPlayer(
                    x,
                    players,
                    maximumPlayerDamage,
                    totalPlayerDamage)))
        };
    }

    private static DamageMeterFragment MappingMobPlayer(
        MobDamageMeterPlayerDto snapshotDto,
        IReadOnlyList<MobDamageMeterPlayerIdentityDto> players,
        long maximumPlayerDamage,
        long totalPlayerDamage)
    {
        if (snapshotDto is null)
        {
            return new DamageMeterFragment();
        }

        var player = snapshotDto.PlayerId > 0 && snapshotDto.PlayerId <= players.Count
            ? players[snapshotDto.PlayerId - 1]
            : null;
        var spellDtos = snapshotDto.Spells ?? [];
        var maximumSpellDamage = spellDtos.Count > 0 ? spellDtos.Max(x => x.DamageHealValue) : 0;
        return new DamageMeterFragment
        {
            Name = player?.Name ?? snapshotDto.Name ?? string.Empty,
            CauserGuid = player?.CauserGuid ?? snapshotDto.CauserGuid,
            CombatTime = snapshotDto.CombatTime,
            Damage = snapshotDto.Damage,
            Dps = CalculateDps(snapshotDto.Damage, snapshotDto.CombatTime),
            DamageInPercent = CalculatePercentage(snapshotDto.Damage, maximumPlayerDamage),
            DamagePercentage = CalculatePercentage(snapshotDto.Damage, totalPlayerDamage),
            CauserMainHand = ItemController.GetItemByUniqueName(snapshotDto.CauserMainHandItemUniqueName ?? string.Empty),
            DamageMeterStyleFragmentType = DamageMeterStyleFragmentType.Damage,
            Spells = new ObservableCollection<UsedSpellFragment>(
                spellDtos.Select(x => MappingMobSpell(
                    x,
                    maximumSpellDamage,
                    snapshotDto.Damage)))
        };
    }

    private static UsedSpellFragment MappingMobSpell(
        MobDamageMeterSpellDto snapshotDto,
        long maximumSpellDamage,
        long totalSpellDamage)
    {
        if (snapshotDto is null)
        {
            return new UsedSpellFragment();
        }

        var spellData = SpellData.GetSpellByIndex(snapshotDto.SpellIndex);
        var uniqueName = snapshotDto.SpellIndex <= 0 ? "AUTO_ATTACK" : spellData.UniqueName;
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            uniqueName = snapshotDto.UniqueName ?? string.Empty;
        }
        var item = !string.IsNullOrWhiteSpace(snapshotDto.ItemUniqueName)
            ? ItemController.GetItemByUniqueName(snapshotDto.ItemUniqueName)
            : ItemController.GetItemByIndex(snapshotDto.ItemIndex);

        return new UsedSpellFragment
        {
            SpellIndex = snapshotDto.SpellIndex,
            ItemIndex = item?.Index ?? 0,
            UniqueName = uniqueName,
            DamageHealValue = snapshotDto.DamageHealValue,
            Target = !string.IsNullOrWhiteSpace(spellData.Target)
                ? spellData.Target
                : snapshotDto.Target ?? string.Empty,
            Category = !string.IsNullOrWhiteSpace(spellData.Category)
                ? spellData.Category
                : snapshotDto.Category ?? string.Empty,
            Ticks = snapshotDto.Ticks,
            DamageInPercent = CalculatePercentage(snapshotDto.DamageHealValue, maximumSpellDamage),
            DamagePercentage = CalculatePercentage(snapshotDto.DamageHealValue, totalSpellDamage),
            HealthChangeType = HealthChangeType.Damage
        };
    }

    private static string ResolveMobName(
        GameFileData.Models.MobJsonObject mobData,
        string uniqueName,
        string savedName,
        long mobObjectId)
    {
        var localizedName = MobsData.GetLocalizedMobName(mobData);
        if (!string.IsNullOrWhiteSpace(localizedName))
        {
            return localizedName;
        }

        if (!string.IsNullOrWhiteSpace(savedName))
        {
            return savedName;
        }

        return !string.IsNullOrWhiteSpace(uniqueName) ? uniqueName : mobObjectId.ToString();
    }

    private static string ResolveMobType(GameFileData.Models.MobJsonObject mobData)
    {
        var faction = MobsData.GetFaction(mobData);
        return string.IsNullOrWhiteSpace(faction)
            ? LocalizationController.Translation("MOB")
            : faction.Replace('_', ' ');
    }

    private static double CalculateDps(long damage, TimeSpan combatTime)
    {
        return damage / Math.Max(1, combatTime.TotalSeconds);
    }

    private static double CalculatePercentage(long value, long total)
    {
        return total > 0 ? (double) value / total * 100 : 0;
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
