using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class DamageStatsSnapshotFactory
{
    private const int TopCount = 5;

    public static DamageStatsSnapshot Clone(DamageStatsSnapshot snapshot)
    {
        snapshot ??= DamageStatsSnapshot.Empty;

        return new DamageStatsSnapshot
        {
            TopSingleHits = snapshot.TopSingleHits.ToList(),
            TopSingleHeals = snapshot.TopSingleHeals.ToList(),
            TopLastHits = snapshot.TopLastHits.ToList(),
            TopOverheals = snapshot.TopOverheals.ToList(),
            TopTakenDamage = snapshot.TopTakenDamage.ToList(),
            TopBurstDamageFiveSeconds = snapshot.TopBurstDamageFiveSeconds.ToList(),
            TopBurstDamageTenSeconds = snapshot.TopBurstDamageTenSeconds.ToList(),
            TopAttackedTargets = snapshot.TopAttackedTargets.ToList()
        };
    }

    public static DamageStatsSnapshot FromSnapshotFragments(IEnumerable<DamageMeterSnapshotFragment> fragments)
    {
        var snapshotFragments = fragments?.ToList() ?? [];

        return new DamageStatsSnapshot
        {
            TopSingleHits = CreateTopSpellEntries(snapshotFragments, HealthChangeType.Damage),
            TopSingleHeals = CreateTopSpellEntries(snapshotFragments.Where(x => x.Heal > 0), HealthChangeType.Heal),
            TopTakenDamage = CreateTopEntries(snapshotFragments, x => x.TakenDamage)
        };
    }

    public static IReadOnlyList<DamageStatsEntry> CreateTopTakenDamageEntries(IEnumerable<DamageMeterFragment> fragments)
    {
        return CreateTopEntries(fragments, x => x.TakenDamage);
    }

    public static IReadOnlyList<DamageStatsEntry> CreateTopTakenDamageEntries(IEnumerable<PlayerGameObject> players)
    {
        return CreateTopEntries(players, x => x.TakenDamage);
    }

    private static IReadOnlyList<DamageStatsEntry> CreateTopEntries<T>(IEnumerable<T> fragments, Func<T, long> valueSelector)
        where T : class
    {
        var rank = 1;
        return (fragments ?? [])
            .Select(x => new DamageStatsEntry
            {
                PlayerName = GetPlayerName(x),
                Value = valueSelector(x)
            })
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.PlayerName)
            .Take(TopCount)
            .Select(x => new DamageStatsEntry
            {
                Rank = rank++,
                PlayerName = x.PlayerName,
                Value = x.Value
            })
            .ToList();
    }

    private static IReadOnlyList<DamageStatsEntry> CreateTopSpellEntries(IEnumerable<DamageMeterSnapshotFragment> fragments, HealthChangeType healthChangeType)
    {
        var rank = 1;
        return fragments
            .Select(x => new DamageStatsEntry
            {
                PlayerName = x.Name,
                Value = x.Spells
                    .Where(y => y.HealthChangeType == healthChangeType)
                    .Select(y => y.DamageHealValue)
                    .DefaultIfEmpty(0)
                    .Max()
            })
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.PlayerName)
            .Take(TopCount)
            .Select(x => new DamageStatsEntry
            {
                Rank = rank++,
                PlayerName = x.PlayerName,
                Value = x.Value
            })
            .ToList();
    }

    private static string GetPlayerName<T>(T fragment)
    {
        return fragment switch
        {
            DamageMeterFragment damageMeterFragment => damageMeterFragment.Name,
            DamageMeterSnapshotFragment snapshotFragment => snapshotFragment.Name,
            PlayerGameObject playerGameObject => playerGameObject.Name,
            _ => string.Empty
        };
    }
}