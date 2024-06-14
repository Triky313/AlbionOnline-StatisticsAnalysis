using System.Collections.Generic;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class SpellLocalization
{
    public string UniqueName { get; init; }
    public Dictionary<string, string> LocalizedNames { get; init; }
    public Dictionary<string, string> LocalizedDescriptions { get; init; }
}