using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerCache
{
    public DateTime CreatedUtc { get; set; }
    public DateTime LastSuccessfulSyncUtc { get; set; }
    public DateTime LastPriceSyncUtc { get; set; }
    public List<DateOnly> ObservedDays { get; set; } = [];
    public List<LossExplorerDailyEventCount> DailyEventCounts { get; set; } = [];
    public List<LossExplorerDailyItem> DailyItems { get; set; } = [];
    public List<LossExplorerProcessedEvent> RecentProcessedEvents { get; set; } = [];
    public List<LossExplorerCachedItem> Items { get; set; } = [];
}