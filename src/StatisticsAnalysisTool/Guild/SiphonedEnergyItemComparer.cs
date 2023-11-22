using System.Collections;

namespace StatisticsAnalysisTool.Guild;

public class SiphonedEnergyItemComparer : IComparer
{
    int IComparer.Compare(object x, object y)
    {
        if (x is SiphonedEnergyItem siphonedEnergyItemX && y is SiphonedEnergyItem siphonedEnergyItemY)
        {
            return siphonedEnergyItemY.Timestamp.CompareTo(siphonedEnergyItemX.Timestamp);
        }

        return -1;
    }
}