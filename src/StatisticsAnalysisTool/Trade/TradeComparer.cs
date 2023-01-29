using System;
using System.Collections;

namespace StatisticsAnalysisTool.Trade;

public class TradeComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is not Trade mailA || y is not Trade mailB)
        {
            throw new ArgumentException("Not a Trade object");
        }

        if (mailA.Ticks > mailB.Ticks)
        {
            return -1;
        }

        if (mailA.Ticks == mailB.Ticks)
        {
            return 0;
        }

        return 1;
    }
}
