using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.GameFileData;

namespace StatisticsAnalysisTool.StorageHistory;

public class VaultInfo
{
    public VaultInfo(long? objectId, string locationGuidString, List<Guid> containerGuidList, List<string> containerNames, List<string> containerIconTags)
    {
        ObjectId = objectId;
        LocationGuidString = locationGuidString;
        ContainerGuidList = containerGuidList;
        ContainerNames = containerNames;
        ContainerIconTags = containerIconTags;
    }

    public long? ObjectId { get; set; }
    public string LocationGuidString { get; set; }
    public List<Guid> ContainerGuidList { get; set; }
    public List<string> ContainerNames { get; set; }
    public List<string> ContainerIconTags { get; set; }
    public MapType MapType => WorldData.GetMapType(LocationGuidString);
    public string MainLocationIndex { get; set; }
    public string UniqueClusterName { get; set; }
}