using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel
{
    public class DungeonEventObject
    {
        public int Id { get; set; }
        public DateTime Opened { get; set; }
        public bool IsOpen { get; set; } = false;
        public string UniqueName { get; set; }
        public DungeonEventObjectType ObjectType => DungeonObjectData.GetDungeonEventObjectType(UniqueName);

        public bool IsBossChest { get; set; }
        public ChestRarity Rarity => DungeonObjectData.GetChestRarity(UniqueName);

        public ShrineBuff ShrineBuff => DungeonObjectData.GetShrineBuff(UniqueName);
        public ShrineType ShrineType => DungeonObjectData.GetShrineType(UniqueName);

        public string Hash => $"{Id}{UniqueName}";
    }
}