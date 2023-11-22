using StatisticsAnalysisTool.Dungeon.Models;
using System.Collections;

namespace StatisticsAnalysisTool.Dungeon;

public class DungeonComparer : IComparer
{
    int IComparer.Compare(object x, object y)
    {
        if (x is DungeonBaseFragment dungeonX && y is DungeonBaseFragment dungeonY)
        {
            return dungeonY.EnterDungeonFirstTime.CompareTo(dungeonX.EnterDungeonFirstTime);
        }

        return -1;
    }
}