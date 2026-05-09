using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageStatsTracker
{
    private const int TopCount = 5;
    private readonly object _syncLock = new();
    private readonly Dictionary<Guid, DamageStatsPlayer> _players = new();
    private readonly List<DamageStatsEvent> _damageEvents = [];

    public void RecordDamage(Guid playerGuid, string playerName, long targetObjectId, long value, double newHealthValue)
    {
        if (value <= 0)
        {
            return;
        }

        lock (_syncLock)
        {
            var player = GetOrAddPlayer(playerGuid, playerName);
            player.BiggestHit = Math.Max(player.BiggestHit, value);
            if (targetObjectId > 0)
            {
                player.AttackedTargetObjectIds.Add(targetObjectId);
            }

            if (targetObjectId > 0 && newHealthValue <= 0)
            {
                player.LastHitTargetObjectIds.Add(targetObjectId);
            }

            _damageEvents.Add(new DamageStatsEvent
            {
                Timestamp = DateTime.UtcNow,
                PlayerGuid = playerGuid,
                TargetObjectId = targetObjectId,
                Value = value
            });
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
            var activePlayers = activePlayerGuids?.ToHashSet() ?? [];
            var healingPlayers = healingPlayerGuids?.ToHashSet() ?? [];
            if (activePlayers.Count <= 0)
            {
                return DamageStatsSnapshot.Empty;
            }

            var players = _players.Values
                .Where(x => activePlayers.Contains(x.PlayerGuid))
                .ToList();

            return new DamageStatsSnapshot
            {
                TopSingleHits = CreateTopEntries(players, x => x.BiggestHit),
                TopSingleHeals = CreateTopEntries(players.Where(x => healingPlayers.Contains(x.PlayerGuid)), x => x.BiggestHeal),
                TopLastHits = CreateTopEntries(players, x => x.LastHitTargetObjectIds.Count),
                TopOverheals = CreateTopEntries(players, x => x.Overheal),
                TopBurstDamageFiveSeconds = CreateBurstDamageEntries(activePlayers, TimeSpan.FromSeconds(5)),
                TopBurstDamageTenSeconds = CreateBurstDamageEntries(activePlayers, TimeSpan.FromSeconds(10)),
                TopAttackedTargets = CreateTopEntries(players, x => x.AttackedTargetObjectIds.Count)
            };
        }
    }

    public void Clear()
    {
        lock (_syncLock)
        {
            _players.Clear();
            _damageEvents.Clear();
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

    private static IReadOnlyList<DamageStatsEntry> CreateTopEntries(IEnumerable<DamageStatsPlayer> players, Func<DamageStatsPlayer, long> valueSelector)
    {
        var rank = 1;
        return players
            .Select(x => new DamageStatsEntry
            {
                PlayerName = x.PlayerName,
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

    private IReadOnlyList<DamageStatsEntry> CreateBurstDamageEntries(IReadOnlySet<Guid> activePlayerGuids, TimeSpan window)
    {
        var rank = 1;
        return _damageEvents
            .Where(x => activePlayerGuids.Contains(x.PlayerGuid))
            .GroupBy(x => x.PlayerGuid)
            .Select(x => new DamageStatsEntry
            {
                PlayerName = GetPlayerName(x.Key),
                Value = GetHighestBurstDamage(x.OrderBy(y => y.Timestamp).ToList(), window)
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

    private string GetPlayerName(Guid playerGuid)
    {
        return _players.TryGetValue(playerGuid, out var player) ? player.PlayerName : string.Empty;
    }

    private static long GetHighestBurstDamage(IReadOnlyList<DamageStatsEvent> events, TimeSpan window)
    {
        long highestDamage = 0;
        long currentDamage = 0;
        var startIndex = 0;

        for (var endIndex = 0; endIndex < events.Count; endIndex++)
        {
            currentDamage += events[endIndex].Value;

            while (events[endIndex].Timestamp - events[startIndex].Timestamp > window)
            {
                currentDamage -= events[startIndex].Value;
                startIndex++;
            }

            highestDamage = Math.Max(highestDamage, currentDamage);
        }

        return highestDamage;
    }
}