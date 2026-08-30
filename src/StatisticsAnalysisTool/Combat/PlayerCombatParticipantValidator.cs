using System;

namespace StatisticsAnalysisTool.Combat;

internal static class PlayerCombatParticipantValidator
{
    private const string MobNamePrefix = "@MOB_";

    public static bool IsPlayerName(string name)
    {
        return !string.IsNullOrWhiteSpace(name)
               && !name.StartsWith(MobNamePrefix, StringComparison.OrdinalIgnoreCase);
    }
}