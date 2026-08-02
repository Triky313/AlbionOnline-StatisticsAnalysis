using System;

namespace StatisticsAnalysisTool.EventLogging;

public sealed class LootComparatorSave
{
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public string DirectoryPath { get; init; } = string.Empty;
    public string ChestLogFilePath { get; init; } = string.Empty;
    public string LootLogFilePath { get; init; } = string.Empty;
    public int ChestLogEntryCount { get; init; }
    public int LootLogEntryCount { get; init; }
}