using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonChestObject
    {
        public int Id { get; set; }
        public bool IsBossChest { get; set; }
        public bool IsChestOpen { get; set; }
        public DateTime Opened { get; set; }
        public ChestRarity Rarity => LootChestData.GetChestRarity(UniqueName);
        public ChestType Type => LootChestData.GetChestType(UniqueName);
        public string UniqueName { get; set; }
    }
}