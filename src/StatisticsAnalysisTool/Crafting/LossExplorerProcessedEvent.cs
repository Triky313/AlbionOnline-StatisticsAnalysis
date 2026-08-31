using System;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerProcessedEvent
{
    public long EventId { get; set; }
    public DateTime TimeStampUtc { get; set; }
}