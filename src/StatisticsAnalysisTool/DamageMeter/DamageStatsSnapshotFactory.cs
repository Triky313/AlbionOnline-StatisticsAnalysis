using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class DamageStatsSnapshotFactory
{
    public static DamageStatsSnapshot Clone(DamageStatsSnapshot snapshot)
    {
        snapshot ??= DamageStatsSnapshot.Empty;

        return new DamageStatsSnapshot
        {
            TopSingleHits = NormalizeEntries(snapshot.TopSingleHits),
            TopSingleHeals = NormalizeEntries(snapshot.TopSingleHeals),
            TopTotalDamage = NormalizeEntries(snapshot.TopTotalDamage),
            TopEffectiveHealing = NormalizeEntries(snapshot.TopEffectiveHealing),
            TopLastHits = NormalizeEntries(snapshot.TopLastHits),
            TopMobKillContribution = NormalizeEntries(snapshot.TopMobKillContribution, true),
            TopOverheals = NormalizeEntries(snapshot.TopOverheals),
            TopTakenDamage = NormalizeEntries(snapshot.TopTakenDamage),
            TopBurstDamageFiveSeconds = NormalizeEntries(snapshot.TopBurstDamageFiveSeconds),
            TopBurstDamageTenSeconds = NormalizeEntries(snapshot.TopBurstDamageTenSeconds),
            TopAttackedTargets = NormalizeEntries(snapshot.TopAttackedTargets),
            TrackedFightCount = snapshot.TrackedFightCount,
            TrackedFightDuration = snapshot.TrackedFightDuration
        };
    }

    public static DamageStatsSnapshot WithSnapshotFragmentFallback(
        DamageStatsSnapshot snapshot,
        IEnumerable<DamageMeterSnapshotFragment> fragments)
    {
        var fallback = FromSnapshotFragments(fragments);
        snapshot ??= DamageStatsSnapshot.Empty;

        return new DamageStatsSnapshot
        {
            TopSingleHits = SelectEntries(snapshot.TopSingleHits, fallback.TopSingleHits),
            TopSingleHeals = SelectEntries(snapshot.TopSingleHeals, fallback.TopSingleHeals),
            TopTotalDamage = SelectEntries(snapshot.TopTotalDamage, fallback.TopTotalDamage),
            TopEffectiveHealing = SelectEntries(snapshot.TopEffectiveHealing, fallback.TopEffectiveHealing),
            TopLastHits = NormalizeEntries(snapshot.TopLastHits),
            TopMobKillContribution = NormalizeEntries(snapshot.TopMobKillContribution, true),
            TopOverheals = NormalizeEntries(snapshot.TopOverheals),
            TopTakenDamage = SelectEntries(snapshot.TopTakenDamage, fallback.TopTakenDamage),
            TopBurstDamageFiveSeconds = NormalizeEntries(snapshot.TopBurstDamageFiveSeconds),
            TopBurstDamageTenSeconds = NormalizeEntries(snapshot.TopBurstDamageTenSeconds),
            TopAttackedTargets = NormalizeEntries(snapshot.TopAttackedTargets),
            TrackedFightCount = snapshot.TrackedFightCount,
            TrackedFightDuration = snapshot.TrackedFightDuration
        };
    }

    public static DamageStatsSnapshot FromLiveData(
        DamageStatsSnapshot trackerSnapshot,
        IEnumerable<PlayerGameObject> players,
        IEnumerable<CombatEvent> combatEvents)
    {
        trackerSnapshot ??= DamageStatsSnapshot.Empty;
        var trackedFights = (combatEvents ?? [])
            .Where(x => x.Contributions.Count > 0)
            .ToList();

        return new DamageStatsSnapshot
        {
            TopSingleHits = trackerSnapshot.TopSingleHits,
            TopSingleHeals = trackerSnapshot.TopSingleHeals,
            TopTotalDamage = trackerSnapshot.TopTotalDamage,
            TopEffectiveHealing = trackerSnapshot.TopEffectiveHealing,
            TopLastHits = trackerSnapshot.TopLastHits,
            TopMobKillContribution = trackerSnapshot.TopMobKillContribution,
            TopOverheals = trackerSnapshot.TopOverheals,
            TopTakenDamage = CreateTopTakenDamageEntries(players),
            TopBurstDamageFiveSeconds = trackerSnapshot.TopBurstDamageFiveSeconds,
            TopBurstDamageTenSeconds = trackerSnapshot.TopBurstDamageTenSeconds,
            TopAttackedTargets = trackerSnapshot.TopAttackedTargets,
            TrackedFightCount = trackedFights.Count,
            TrackedFightDuration = trackedFights
                .Select(GetDuration)
                .Aggregate(TimeSpan.Zero, (total, duration) => total + duration)
        };
    }

    public static DamageStatsSnapshot FromSnapshotFragments(IEnumerable<DamageMeterSnapshotFragment> fragments)
    {
        var snapshotFragments = fragments?.ToList() ?? [];

        return new DamageStatsSnapshot
        {
            TopSingleHits = CreateTopSpellEntries(snapshotFragments, HealthChangeType.Damage),
            TopSingleHeals = CreateTopSpellEntries(snapshotFragments.Where(x => x.Heal > 0), HealthChangeType.Heal),
            TopTotalDamage = CreateTopEntries(snapshotFragments, x => x.Damage),
            TopEffectiveHealing = CreateTopEntries(snapshotFragments, x => x.Heal),
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
        return DamageStatsEntryFactory.Rank((fragments ?? [])
            .Select(x => new DamageStatsEntry
            {
                PlayerName = GetPlayerName(x),
                Value = valueSelector(x)
            }));
    }

    private static IReadOnlyList<DamageStatsEntry> CreateTopSpellEntries(
        IEnumerable<DamageMeterSnapshotFragment> fragments,
        HealthChangeType healthChangeType)
    {
        return DamageStatsEntryFactory.Rank(fragments
            .Select(x => new DamageStatsEntry
            {
                PlayerName = x.Name,
                Value = x.Spells
                    .Where(y => y.HealthChangeType == healthChangeType)
                    .Select(y => y.DamageHealValue)
                    .DefaultIfEmpty(0)
                    .Max()
            }));
    }

    private static IReadOnlyList<DamageStatsEntry> SelectEntries(
        IReadOnlyList<DamageStatsEntry> entries,
        IReadOnlyList<DamageStatsEntry> fallbackEntries)
    {
        return NormalizeEntries(entries.Count > 0 ? entries : fallbackEntries);
    }

    private static IReadOnlyList<DamageStatsEntry> NormalizeEntries(
        IReadOnlyList<DamageStatsEntry> entries,
        bool calculateSharePercentage = false)
    {
        if (entries == null || entries.Count == 0)
        {
            return [];
        }

        return entries.Any(x => x.BarPercentage > 0)
            ? entries.ToList()
            : DamageStatsEntryFactory.Rank(entries, calculateSharePercentage);
    }

    private static TimeSpan GetDuration(CombatEvent combatEvent)
    {
        var endTime = combatEvent.EndTime ?? combatEvent.LastEventTime;
        return endTime > combatEvent.StartTime
            ? endTime - combatEvent.StartTime
            : TimeSpan.Zero;
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
