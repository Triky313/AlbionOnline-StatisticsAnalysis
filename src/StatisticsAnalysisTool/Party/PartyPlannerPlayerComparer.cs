using System;
using System.Collections;

namespace StatisticsAnalysisTool.Party;

public class PartyPlannerPlayerComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is not PartyPlayer player1 || y is not PartyPlayer player2)
        {
            throw new ArgumentException("Not a PartyPlannerPlayer object");
        }

        return string.CompareOrdinal(player1.Username, player2.Username);
    }
}