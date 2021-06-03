using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonChestObject : DungeonEventObject
    {
        public bool IsBossChest { get; set; }
        public ChestRarity Rarity => DungeonObjectData.GetChestRarity(UniqueName);
        public ChestType Type => DungeonObjectData.GetChestType(UniqueName);
    }
}