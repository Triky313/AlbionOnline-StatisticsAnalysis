using StatisticsAnalysisTool.DungeonTracker;
using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class DungeonEventObject
{
    public int Id { get; set; }
    public DateTime Opened { get; set; }
    public bool IsOpen { get; set; } = false;
    public string UniqueName { get; set; }
    public DungeonEventObjectType ObjectType => DungeonController.GetDungeonEventObjectType(UniqueName);

    public bool IsBossChest { get; set; }
    public TreasureRarity Rarity => DungeonController.GetChestRarity(UniqueName);

    public ShrineBuff ShrineBuff => DungeonController.GetShrineBuff(UniqueName);
    public ShrineType ShrineType => DungeonController.GetShrineType(UniqueName);

    public string Hash => $"{Id}{UniqueName}";
}