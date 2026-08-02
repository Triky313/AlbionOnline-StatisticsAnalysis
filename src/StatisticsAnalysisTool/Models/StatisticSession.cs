using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Models;

public sealed class StatisticSession
{
    public Guid Id { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public DateTime? EndedAtUtc { get; set; }
    public string CharacterName { get; set; } = string.Empty;
    public ServerLocation ServerLocation { get; set; }
}