using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonChest
    {
        public int Id { get; set; }
        public DateTime Discovered { get; set; }
        public string UniqueName { get; set; }
        public ChestType Type => LootChestController.GetChestType(UniqueName);
        public ChestRarity Rarity => LootChestController.GetChestRarity(UniqueName);
        public bool IsBossChest { get; set; }
        public bool IsChestOpen { get; set; }
    }
}