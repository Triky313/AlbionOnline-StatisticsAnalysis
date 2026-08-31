using System;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerDailyEventCount
{
    public DateOnly Day { get; set; }
    public long EventCount { get; set; }
}