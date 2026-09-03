using System;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterSnapshotIndexEntryDto
{
    public Guid Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Location { get; set; } = string.Empty;
    public bool IsAutoSave { get; set; }
}