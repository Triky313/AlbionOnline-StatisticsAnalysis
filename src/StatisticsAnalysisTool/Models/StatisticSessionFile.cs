using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models;

public sealed class StatisticSessionFile
{
    public StatisticSession Session { get; set; } = new();
    public List<StatisticEntry> Entries { get; set; } = [];
}