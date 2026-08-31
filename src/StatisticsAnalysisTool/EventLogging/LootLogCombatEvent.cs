using System;

namespace StatisticsAnalysisTool.EventLogging;

public sealed class LootLogCombatEvent
{
    public DateTime UtcTimestamp { get; init; }
    public string DiedName { get; init; } = string.Empty;
    public string DiedPlayerGuild { get; init; } = string.Empty;
    public string KilledByName { get; init; } = string.Empty;
    public string KilledByGuild { get; init; } = string.Empty;
    public string ClusterName { get; init; } = string.Empty;

    public bool IsForPlayer(string playerName)
    {
        return !string.IsNullOrWhiteSpace(playerName)
               && (string.Equals(DiedName, playerName, StringComparison.OrdinalIgnoreCase)
                   || string.Equals(KilledByName, playerName, StringComparison.OrdinalIgnoreCase));
    }
}