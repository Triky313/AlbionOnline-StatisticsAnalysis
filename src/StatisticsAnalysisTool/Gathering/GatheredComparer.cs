using System;
using System.Collections;

namespace StatisticsAnalysisTool.Gathering;

public class GatheredComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is not Gathered gatheredA || y is not Gathered gatheredB)
        {
            throw new ArgumentException("Not a Gathered object");
        }

        if (gatheredA.Timestamp > gatheredB.Timestamp)
        {
            return -1;
        }

        if (gatheredA.Timestamp == gatheredB.Timestamp)
        {
            return 0;
        }

        return 1;
    }
}
