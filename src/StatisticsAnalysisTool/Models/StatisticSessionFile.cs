using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models;

public sealed class StatisticSessionFile
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; set; } = CurrentSchemaVersion;
    public StatisticSession Session { get; set; } = new();
    public List<StatisticEntry> Entries { get; set; } = [];
}