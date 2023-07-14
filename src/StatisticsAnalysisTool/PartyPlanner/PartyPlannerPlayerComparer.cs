using System;
using System.Collections;

namespace StatisticsAnalysisTool.PartyPlanner;

public class PartyPlannerPlayerComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is not PartyBuilderPlayer player1 || y is not PartyBuilderPlayer player2)
        {
            throw new ArgumentException("Not a PartyPlannerPlayer object");
        }

        return string.CompareOrdinal(player1.Username, player2.Username);
    }
}