using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonChest
    {
        public int Id { get; set; }
        public ChestType Type { get; set; }
        public ChestRarity Rarity { get; set; }
        public bool IsChestOpen { get; set; }
    }
}