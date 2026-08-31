using System;
using System.Collections.Generic;
using System.Linq;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageStatsTracker
{
    private readonly object _syncLock = new();
    private readonly Dictionary<Guid, DamageStatsPlayer> _players = new();

    public void RecordDamage(
        Guid playerGuid,
        string playerName,
        long targetObjectId,
        long value,
        double newHealthValue,
        bool isMobTarget,
        int causingSpellIndex,
        DamageType damageType)
    {
        if (value <= 0)
        {
            return;
        }

        lock (_syncLock)
        {
            var player = GetOrAddPlayer(playerGuid, playerName);
            var damageEvent = new DamageStatsEvent(DateTime.UtcNow, value);
            player.BiggestHit = Math.Max(player.BiggestHit, value);
            player.TotalDamage += value;
            player.BurstDamageFiveSeconds.AddDamage(damageEvent);
            player.BurstDamageTenSeconds.AddDamage(damageEvent);
            if (damageType != DamageType.Unknown)
            {
                player.DamageByType[damageType] = player.DamageByType.GetValueOrDefault(damageType) + value;
            }

            var spellIndex = Math.Max(0, causingSpellIndex);
            player.DamageBySpellIndex[spellIndex] = player.DamageBySpellIndex.GetValueOrDefault(spellIndex) + value;

            if (targetObjectId > 0)
            {
                player.AttackedTargetObjectIds.Add(targetObjectId);
            }

            if (targetObjectId > 0 && newHealthValue <= 0)
            {
                player.LastHitTargetObjectIds.Add(targetObjectId);
                if (isMobTarget)
                {
                    player.MobLastHitTargetObjectIds.Add(targetObjectId);
                }
            }
        }
    }

    public void RecordHeal(Guid playerGuid, string playerName, long value)
    {
        if (value <= 0)
        {
            return;
        }

        lock (_syncLock)
        {
            var player = GetOrAddPlayer(playerGuid, playerName);
            player.BiggestHeal = Math.Max(player.BiggestHeal, value);
            player.EffectiveHealing += value;
        }
    }

    public void RecordOverheal(Guid playerGuid, string playerName, long value)
    {
        if (value <= 0)
        {
            return;
        }

        lock (_syncLock)
        {
            var player = GetOrAddPlayer(playerGuid, playerName);
            player.Overheal += value;
        }
    }

    public DamageStatsSnapshot CreateSnapshot(IEnumerable<Guid> activePlayerGuids, IEnumerable<Guid> healingPlayerGuids)
    {
        lock (_syncLock)
        {
            var trackedPlayers = _players.Values.ToList();
            if (trackedPlayers.Count == 0)
            {
                return DamageStatsSnapshot.Empty;
            }

            var activePlayers = ResolveTrackedPlayerGuids(activePlayerGuids, trackedPlayers);
            var healingPlayers = ResolveHealingPlayerGuids(healingPlayerGuids, trackedPlayers);
            var players = trackedPlayers.Where(x => activePlayers.Contains(x.PlayerGuid)).ToList();

            return new DamageStatsSnapshot
            {
                TopSingleHits = CreateTopEntries(players, x => x.BiggestHit),
                TopSingleHeals = CreateTopEntries(players.Where(x => healingPlayers.Contains(x.PlayerGuid)), x => x.BiggestHeal),
                TopTotalDamage = CreateTopEntries(players, x => x.TotalDamage),
                TopEffectiveHealing = CreateTopEntries(players, x => x.EffectiveHealing),
                TopLastHits = CreateTopEntries(players, x => x.LastHitTargetObjectIds.Count),
                TopMobKillContribution = CreateTopEntries(players, x => x.MobLastHitTargetObjectIds.Count, true),
                TopOverheals = CreateTopEntries(players, x => x.Overheal),
                TopBurstDamageFiveSeconds = CreateTopEntries(players, x => x.BurstDamageFiveSeconds.HighestDamage),
                TopBurstDamageTenSeconds = CreateTopEntries(players, x => x.BurstDamageTenSeconds.HighestDamage),
                TopAttackedTargets = CreateTopEntries(players, x => x.AttackedTargetObjectIds.Count),
                DamageTypeTotals = CreateDamageTypeEntries(players),
                TopDamageSpells = CreateDamageSpellEntries(players)
            };
        }
    }

    private static IReadOnlySet<Guid> ResolveTrackedPlayerGuids(
        IEnumerable<Guid> activePlayerGuids,
        IReadOnlyCollection<DamageStatsPlayer> trackedPlayers)
    {
        var trackedPlayerGuids = trackedPlayers.Select(player => player.PlayerGuid).ToHashSet();
        var matchingPlayerGuids = (activePlayerGuids ?? [])
            .Where(trackedPlayerGuids.Contains)
            .ToHashSet();

        return matchingPlayerGuids.Count > 0 ? matchingPlayerGuids : trackedPlayerGuids;
    }

    private static IReadOnlySet<Guid> ResolveHealingPlayerGuids(
        IEnumerable<Guid> healingPlayerGuids,
        IReadOnlyCollection<DamageStatsPlayer> trackedPlayers)
    {
        var trackedHealingPlayerGuids = trackedPlayers
            .Where(player => player.EffectiveHealing > 0 || player.BiggestHeal > 0)
            .Select(player => player.PlayerGuid)
            .ToHashSet();
        var matchingPlayerGuids = (healingPlayerGuids ?? [])
            .Where(trackedHealingPlayerGuids.Contains)
            .ToHashSet();

        return matchingPlayerGuids.Count > 0 ? matchingPlayerGuids : trackedHealingPlayerGuids;
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            _players.Clear();
        }
    }

    private DamageStatsPlayer GetOrAddPlayer(Guid playerGuid, string playerName)
    {
        if (_players.TryGetValue(playerGuid, out var player))
        {
            player.PlayerName = playerName;
            return player;
        }

        player = new DamageStatsPlayer
        {
            PlayerGuid = playerGuid,
            PlayerName = playerName
        };

        _players.Add(playerGuid, player);
        return player;
    }

    private static IReadOnlyList<DamageStatsEntry> CreateTopEntries(
        IEnumerable<DamageStatsPlayer> players,
        Func<DamageStatsPlayer, long> valueSelector,
        bool calculateSharePercentage = false)
    {
        return DamageStatsEntryFactory.Rank(
            players.Select(x => new DamageStatsEntry
            {
                PlayerName = x.PlayerName,
                Value = valueSelector(x)
            }),
            calculateSharePercentage);
    }

    private static IReadOnlyList<DamageTypeStatsEntry> CreateDamageTypeEntries(IEnumerable<DamageStatsPlayer> players)
    {
        return DamageTypeStatsEntryFactory.Rank(players
            .SelectMany(player => player.DamageByType)
            .GroupBy(entry => entry.Key)
            .Select(group => new DamageTypeStatsEntry
            {
                DamageType = group.Key,
                Value = group.Sum(entry => entry.Value)
            }));
    }

    private static IReadOnlyList<DamageSpellStatsEntry> CreateDamageSpellEntries(IEnumerable<DamageStatsPlayer> players)
    {
        return DamageSpellStatsEntryFactory.Rank(players
            .SelectMany(player => player.DamageBySpellIndex)
            .Select(entry => new DamageSpellStatsEntry
            {
                SpellIndex = entry.Key,
                Value = entry.Value
            }));
    }
}
