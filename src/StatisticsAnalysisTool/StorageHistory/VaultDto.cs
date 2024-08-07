using StatisticsAnalysisTool.Cluster;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.StorageHistory;

public class VaultDto
{
    public string Location { get; set; }
    public string MainLocationIndex { get; set; }
    public MapType MapType { get; set; }
    public List<VaultContainerDto> VaultContainer { get; set; } = new();
}