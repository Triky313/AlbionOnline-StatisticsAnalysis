using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonShrineObject : DungeonEventObject
    {
        public ShrineBuff Buff => DungeonObjectData.GetShrineBuff(UniqueName);
        public ShrineType Type => DungeonObjectData.GetShrineType(UniqueName);
    }
}