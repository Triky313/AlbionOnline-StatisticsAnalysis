using System;

namespace StatisticsAnalysisTool.EventLogging;

public sealed class LootComparatorSaveMetadata
{
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public int ChestLogEntryCount { get; init; }
    public int LootLogEntryCount { get; init; }
}