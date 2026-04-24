using System;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public sealed class RandomDungeonExitInfo
{
    public int ObjectId { get; init; }
    public WorldPosition? SourceExitPosition { get; init; }
    public string SourceClusterIndex { get; init; } = string.Empty;
    public int Level { get; init; } = -1;
    public bool IsAlreadyEntered { get; init; }
    public DateTime LastSeenUtc { get; init; } = DateTime.UtcNow;
    public bool HasVisibleLevel => !IsAlreadyEntered && Level is >= 0 and <= 4;
}