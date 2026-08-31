using StatisticsAnalysisTool.Models;
using System;

namespace StatisticsAnalysisTool.Combat;

internal sealed class PendingPlayerKill(
    long victimObjectId,
    string victimName,
    CombatPlayerSnapshot killer,
    CombatPlayerSnapshot victim,
    DateTime occurredAtUtc)
{
    public CombatPlayerSnapshot Killer { get; } = killer;
    public CombatPlayerSnapshot Victim { get; } = victim;
    public DateTime OccurredAtUtc { get; } = occurredAtUtc;

    public bool Matches(long playerObjectId, string playerName)
    {
        if (victimObjectId > 0 && playerObjectId > 0)
        {
            return victimObjectId == playerObjectId;
        }

        return !string.IsNullOrWhiteSpace(victimName)
               && !string.IsNullOrWhiteSpace(playerName)
               && string.Equals(victimName, playerName, StringComparison.OrdinalIgnoreCase);
    }
}