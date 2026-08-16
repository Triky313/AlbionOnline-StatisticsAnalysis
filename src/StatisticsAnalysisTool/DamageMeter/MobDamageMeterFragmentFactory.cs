using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class MobDamageMeterFragmentFactory
{
    public static IReadOnlyList<MobDamageMeterFragment> Create(
        IReadOnlyCollection<CombatMobDamageStats> mobStats,
        Func<Guid, Item> mainHandResolver,
        Func<Guid, int, int> spellItemResolver)
    {
        if (mobStats == null || mobStats.Count == 0)
        {
            return [];
        }

        var maxMobDamage = mobStats.Max(x => x.Damage);
        var totalMobDamage = mobStats.Sum(x => x.Damage);

        return mobStats
            .OrderByDescending(x => x.Damage)
            .ThenByDescending(x => x.LastDamageTime)
            .Select(x => CreateMobFragment(x, maxMobDamage, totalMobDamage, mainHandResolver, spellItemResolver))
            .ToList();
    }

    private static MobDamageMeterFragment CreateMobFragment(
        CombatMobDamageStats mobStats,
        long maxMobDamage,
        long totalMobDamage,
        Func<Guid, Item> mainHandResolver,
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
            MobTier = mobData.Tier,
            MobType = ResolveMobType(MobsData.GetFaction(mobData)),
            MobRank = ResolveMobRank(mobStats.UniqueName),
            ContentTypeName = LocalizationController.Translation(DashboardContentTypeResolver.GetTranslationKey(mobStats.ContentType)),
            MapName = string.IsNullOrWhiteSpace(mobStats.ClusterName) ? mobStats.ClusterKey : mobStats.ClusterName,
            MapTier = mobStats.MapTier,
            AvatarSource = MobAvatarImageProvider.GetAvatarSource(MobsData.GetAvatarFileName(mobData)),
            Damage = mobStats.Damage,
            DamageInPercent = CalculatePercentage(mobStats.Damage, maxMobDamage),
            DamagePercentage = CalculatePercentage(mobStats.Damage, totalMobDamage),
            Players = CreatePlayerFragments(mobStats.Players, mainHandResolver, spellItemResolver)
        };
    }

    private static ObservableCollection<DamageMeterFragment> CreatePlayerFragments(
        IReadOnlyCollection<CombatMobPlayerDamageStats> playerStats,
        Func<Guid, Item> mainHandResolver,
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
                    CauserMainHand = mainHandResolver(x.PlayerGuid),
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

    private static string ResolveMobRank(string uniqueName)
    {
        if (string.IsNullOrWhiteSpace(uniqueName))
        {
            return LocalizationController.Translation("NORMAL");
        }

        var normalizedUniqueName = uniqueName.ToUpperInvariant();
        if (normalizedUniqueName.Contains("_BOSS") || normalizedUniqueName.Contains("_MINIBOSS"))
        {
            return LocalizationController.Translation("BOSS");
        }

        return normalizedUniqueName.Contains("ELITE")
            ? LocalizationController.Translation("ELITE")
            : LocalizationController.Translation("NORMAL");
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
