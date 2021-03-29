using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonChestObject
    {
        public int Id { get; set; }
        public bool IsBossChest { get; set; }
        public bool IsChestOpen { get; set; }
        public DateTime Opened { get; set; }
        public ChestRarity Rarity { get; set; }
        public ChestStatus Status { get; set; }
        public ChestType Type { get; set; }
        public string UniqueName { get; set; }
    }
}