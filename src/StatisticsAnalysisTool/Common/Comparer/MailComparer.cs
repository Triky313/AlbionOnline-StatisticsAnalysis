using System;
using System.Collections;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.Common.Comparer;

public class MailComparer : IComparer
{
    public int Compare(object x, object y)
    {
        if (x is not Mail mailA || y is not Mail mailB)
        {
            throw new ArgumentException("Not a Mail object");
        }

        if (mailA.Tick > mailB.Tick)
        {
            return -1;
        }

        if (mailA.Tick == mailB.Tick)
        {
            return 0;
        }

        return 1;
    }
}
