using StatisticsAnalysisTool.Enumerations;
using System;

namespace StatisticsAnalysisTool.Cluster;

public sealed class ClusterInfoDto
{
    public DateTime Entered { get; set; }
    public MapType MapType { get; set; } = MapType.Unknown;
    public Guid? Guid { get; set; }
    public string Index { get; set; }
    public string InstanceName { get; set; }
    public string WorldMapDataType { get; set; }
    public byte[] DungeonInformation { get; set; }
    public Tier MistsDungeonTier { get; set; }
    public Tier RandomDungeonTier { get; set; } = Tier.Unknown;
    public int RandomDungeonLevel { get; set; } = -1;
    public string MainClusterIndex { get; set; }
    public string MapHistoryNote { get; set; } = string.Empty;
}