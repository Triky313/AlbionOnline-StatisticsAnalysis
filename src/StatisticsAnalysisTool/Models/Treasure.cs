using System;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models;

public class Treasure
{
    public Treasure()
    {
        OpenedAt = DateTime.UtcNow;
    }

    public TreasureRarity TreasureRarity { get; set; }
    public TreasureType TreasureType { get; set; }
    public Guid? OpenedBy { get; set; }
    public DateTime OpenedAt { get; init; }
}