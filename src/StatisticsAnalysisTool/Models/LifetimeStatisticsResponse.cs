using System;

namespace StatisticsAnalysisTool.Models;

public class LifetimeStatisticsResponse
{
    public PvEResponse PvE { get; set; }
    public GatheringResponse Gathering { get; set; }
    public CraftingResponse Crafting { get; set; }
    public int CrystalLeague { get; set; }
    public DateTime Timestamp { get; set; }
}