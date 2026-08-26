using System;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringSession
{
    public Guid Id { get; init; }
    public DateTime StartedAtUtc { get; init; }
    public string CharacterName { get; init; } = string.Empty;
}