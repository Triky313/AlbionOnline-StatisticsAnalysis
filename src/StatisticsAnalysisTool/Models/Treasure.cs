using System;
using System.Collections.Generic;
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
    public List<Guid> OpenedBy { get; set; }
    public DateTime OpenedAt { get; init; }
}