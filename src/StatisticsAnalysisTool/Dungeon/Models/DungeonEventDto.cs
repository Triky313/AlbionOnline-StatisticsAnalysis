using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class DungeonEventDto
{
    public int Id { get; set; }
    public DateTime Opened { get; set; }
    public bool IsBossChest { get; set; }
    public bool IsChestOpen { get; set; }
    public TreasureRarity Rarity { get; set; }
    public ChestStatus Status { get; set; }
    public EventType Type { get; set; }
    public string UniqueName { get; set; }
    public ShrineType ShrineType { get; set; }
    public ShrineBuff ShrineBuff { get; set; }
}