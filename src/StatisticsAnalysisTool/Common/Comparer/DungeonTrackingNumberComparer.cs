using System.Collections;

namespace StatisticsAnalysisTool.Common.Comparer;

public class DungeonTrackingNumberComparer : IComparer
{
    int IComparer.Compare(object x, object y)
    {
        var numberX = x as int? ?? -1;
        var numberY = y as int? ?? -1;

        if (numberX != -1 && numberY != -1)
        {
            return numberX.CompareTo(numberY);
        }

        return -1;
    }
}