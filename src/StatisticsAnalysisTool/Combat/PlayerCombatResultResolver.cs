using System;

namespace StatisticsAnalysisTool.Combat;

internal static class PlayerCombatResultResolver
{
    public static PlayerCombatResult Resolve(
        long diedPlayerObjectId,
        string diedPlayerName,
        long killerObjectId,
        string killerPlayerName,
        bool isLethal,
        long? localPlayerObjectId,
        string localPlayerName)
    {
        var isLocalVictim = IsLocalPlayer(
            diedPlayerObjectId,
            diedPlayerName,
            localPlayerObjectId,
            localPlayerName);
        var isLocalKiller = IsLocalPlayer(
            killerObjectId,
            killerPlayerName,
            localPlayerObjectId,
            localPlayerName);
        if (!isLocalVictim && !isLocalKiller)
        {
            return PlayerCombatResult.None;
        }

        return (isLocalVictim, isLethal) switch
        {
            (true, true) => PlayerCombatResult.Death,
            (true, false) => PlayerCombatResult.KnockedOut,
            (false, true) => PlayerCombatResult.Kill,
            (false, false) => PlayerCombatResult.Knockout
        };
    }

    private static bool IsLocalPlayer(
        long playerObjectId,
        string playerName,
        long? localPlayerObjectId,
        string localPlayerName)
    {
        if (playerObjectId > 0 && localPlayerObjectId.HasValue)
        {
            return playerObjectId == localPlayerObjectId.Value;
        }

        return !string.IsNullOrWhiteSpace(playerName)
               && !string.IsNullOrWhiteSpace(localPlayerName)
               && string.Equals(playerName, localPlayerName, StringComparison.OrdinalIgnoreCase);
    }
}