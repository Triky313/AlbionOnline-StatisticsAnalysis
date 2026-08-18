using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class MobDamageMeterFragmentFactory
{
    public static IReadOnlyList<MobDamageMeterFragment> Create(
        IReadOnlyCollection<CombatMobDamageStats> mobStats,
        Func<Guid, int, int> spellItemResolver)
    {
        var totalMobDamage = mobStats?.Sum(x => x.Damage) ?? 0;
        return Create(mobStats, totalMobDamage, spellItemResolver);
    }

    public static IReadOnlyList<MobDamageMeterFragment> Create(
        IReadOnlyCollection<CombatMobDamageStats> mobStats,
        long totalMobDamage,
        Func<Guid, int, int> spellItemResolver)
    {
        if (mobStats == null || mobStats.Count == 0)
        {
            return [];
        }

        return mobStats
            .OrderByDescending(x => x.FirstDamageTime)
            .ThenByDescending(x => x.LastDamageTime)
            .Select(x => CreateMobFragment(x, totalMobDamage, spellItemResolver))
            .ToList();
    }

    private static MobDamageMeterFragment CreateMobFragment(
        CombatMobDamageStats mobStats,
        long totalMobDamage,
        Func<Guid, int, int> spellItemResolver)
    {
        var mobData = MobsData.GetMobByUniqueNameOrDefault(mobStats.UniqueName);
        var localizedName = MobsData.GetLocalizedMobName(mobData);
        var name = !string.IsNullOrWhiteSpace(localizedName)
            ? localizedName
            : !string.IsNullOrWhiteSpace(mobStats.UniqueName)
                ? mobStats.UniqueName
                : mobStats.MobObjectId.ToString();
        var combatTime = CalculateCombatTime(mobStats.FirstDamageTime, mobStats.LastDamageTime);
        var durationInSeconds = Math.Max(1, combatTime.TotalSeconds);

        return new MobDamageMeterFragment
        {
            MobInstanceId = mobStats.MobInstanceId,
            MobObjectId = mobStats.MobObjectId,
            Name = name,
            UniqueName = mobStats.UniqueName,
            ClusterName = mobStats.ClusterName,
            FirstAttackTime = mobStats.FirstDamageTime,
            CombatTime = combatTime,
            Dps = mobStats.Damage / durationInSeconds,
            MobTier = (Tier) mobData.Tier,
            MobType = ResolveMobType(MobsData.GetFaction(mobData)),
            MobTypeCategory = mobData.MobTypeCategory ?? string.Empty,
            Category = mobData.Category ?? string.Empty,
            Faction = mobData.Faction ?? string.Empty,
            AttackType = mobData.AttackType ?? string.Empty,
            ContentType = mobStats.ContentType,
            MapName = string.IsNullOrWhiteSpace(mobStats.ClusterName) ? mobStats.ClusterKey : mobStats.ClusterName,
            MapTier = mobStats.MapTier,
            AvatarSource = MobAvatarImageProvider.GetAvatarSource(MobsData.GetAvatarFileName(mobData)),
            Damage = mobStats.Damage,
            DamagePercentage = CalculatePercentage(mobStats.Damage, totalMobDamage),
            Players = CreatePlayerFragments(mobStats.Players, spellItemResolver)
        };
    }

    private static ObservableCollection<DamageMeterFragment> CreatePlayerFragments(
        IReadOnlyCollection<CombatMobPlayerDamageStats> playerStats,
        Func<Guid, int, int> spellItemResolver)
    {
        var maxPlayerDamage = playerStats.Count > 0 ? playerStats.Max(x => x.Damage) : 0;
        var totalPlayerDamage = playerStats.Sum(x => x.Damage);
        var players = playerStats
            .OrderByDescending(x => x.Damage)
            .Select(x =>
            {
                var combatTime = x.LastDamageTime >= x.FirstDamageTime
                    ? x.LastDamageTime - x.FirstDamageTime
                    : TimeSpan.Zero;
                var durationInSeconds = Math.Max(1, combatTime.TotalSeconds);

                return new DamageMeterFragment
                {
                    CauserGuid = x.PlayerGuid,
                    Name = x.PlayerName,
                    CauserMainHand = DamageMeterWeaponResolver.GetWeaponByIndex(x.LastContributionWeaponItemIndex),
                    Damage = x.Damage,
                    DamageInPercent = CalculatePercentage(x.Damage, maxPlayerDamage),
                    DamagePercentage = CalculatePercentage(x.Damage, totalPlayerDamage),
                    Dps = x.Damage / durationInSeconds,
                    CombatTime = combatTime,
                    DamageMeterStyleFragmentType = DamageMeterStyleFragmentType.Damage,
                    Spells = CreateSpellFragments(x, spellItemResolver)
                };
            });

        return new ObservableCollection<DamageMeterFragment>(players);
    }

    private static ObservableCollection<UsedSpellFragment> CreateSpellFragments(
        CombatMobPlayerDamageStats playerStats,
        Func<Guid, int, int> spellItemResolver)
    {
        var maxSpellDamage = playerStats.DamageBySpell.Count > 0
            ? playerStats.DamageBySpell.Values.Max()
            : 0;
        var spells = playerStats.DamageBySpell
            .OrderByDescending(x => x.Value)
            .Select(x =>
            {
                var spellData = SpellData.GetSpellByIndex(x.Key);
                return new UsedSpellFragment
                {
                    SpellIndex = x.Key,
                    ItemIndex = spellItemResolver(playerStats.PlayerGuid, x.Key),
                    UniqueName = x.Key == 0 ? "AUTO_ATTACK" : spellData.UniqueName,
                    Target = spellData.Target,
                    Category = spellData.Category,
                    DamageHealValue = x.Value,
                    DamageInPercent = CalculatePercentage(x.Value, maxSpellDamage),
                    DamagePercentage = CalculatePercentage(x.Value, playerStats.Damage),
                    HealthChangeType = HealthChangeType.Damage,
                    Ticks = playerStats.HitCountBySpell.GetValueOrDefault(x.Key)
                };
            });

        return new ObservableCollection<UsedSpellFragment>(spells);
    }

    private static string ResolveMobType(string faction)
    {
        return string.IsNullOrWhiteSpace(faction)
            ? LocalizationController.Translation("MOB")
            : faction.Replace('_', ' ');
    }

    private static TimeSpan CalculateCombatTime(DateTime firstDamageTime, DateTime lastDamageTime)
    {
        return firstDamageTime != default && lastDamageTime >= firstDamageTime
            ? lastDamageTime - firstDamageTime
            : TimeSpan.Zero;
    }

    private static double CalculatePercentage(long value, long total)
    {
        return total > 0 ? (double) value / total * 100 : 0;
    }
}
