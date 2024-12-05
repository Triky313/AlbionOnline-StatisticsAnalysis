using System;
using System.Collections;

namespace StatisticsAnalysisTool.EventLogging;

public class LootingPlayerComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is LootingPlayer playerX && y is LootingPlayer playerY)
        {
            return string.Compare(playerX.PlayerName, playerY.PlayerName, StringComparison.Ordinal);
        }

        return 0;
    }
}