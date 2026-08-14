using System;
using StatisticsAnalysisTool.Cluster;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public sealed class RandomDungeonExitInfo
{
    public int ObjectId { get; init; }
    public WorldPosition? SourceExitPosition { get; init; }
    public string SourceClusterIndex { get; init; } = string.Empty;
    public string UniqueName { get; init; } = string.Empty;
    public string DungeonType { get; init; } = string.Empty;
    public int Level { get; init; } = -1;
    public bool IsAlreadyEntered { get; init; }
    public DateTime LastSeenUtc { get; init; } = DateTime.UtcNow;
    public bool HasVisibleLevel => !IsAlreadyEntered && Level is >= 0 and <= 4;
    public MistsRarity ResolvedMistsRarity => UniqueName.StartsWith("MISTS_", StringComparison.Ordinal) ? MistsRarityResolver.FromValue(Level) : MistsRarity.Unknown;
    public MistsType ResolvedMistsType => MistsTypeResolver.FromUniqueName(UniqueName);
    public DragonAreaPortalSize ResolvedDragonAreaPortalSize
    {
        get
        {
            var portalSize = DragonAreaPortalSizeResolver.FromUniqueName(UniqueName);
            return portalSize != DragonAreaPortalSize.Unknown
                ? portalSize
                : DragonAreaPortalSizeResolver.FromUniqueName(DungeonType);
        }
    }
}