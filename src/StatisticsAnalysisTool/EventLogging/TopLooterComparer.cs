using System;
using System.Collections;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.EventLogging;

public class TopLooterComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is not TopLooterObject topLooterObjectA || y is not TopLooterObject topLooterObjectB)
        {
            throw new ArgumentException("Not a TopLooterObject object");
        }

        if (topLooterObjectA.LootActions > topLooterObjectB.LootActions)
        {
            return -1;
        }

        if (topLooterObjectA.LootActions == topLooterObjectB.LootActions)
        {
            if (topLooterObjectA.Quantity > topLooterObjectB.Quantity)
            {
                return -1;
            }

            if (topLooterObjectA.Quantity < topLooterObjectB.Quantity)
            {
                return 1;
            }

            return 0;
        }

        return 1;
    }
}