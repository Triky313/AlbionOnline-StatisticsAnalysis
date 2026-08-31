using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Crafting;

public sealed class LossExplorerEvent
{
    public long EventId { get; init; }
    public DateTime TimeStampUtc { get; init; }
    public IReadOnlyList<LossExplorerEventItem> EquipmentItems { get; init; } = [];
    public IReadOnlyList<LossExplorerEventItem> InventoryItems { get; init; } = [];
}