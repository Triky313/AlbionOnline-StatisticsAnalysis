using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class VaultInfo
{
    public VaultInfo(long? objectId, VaultLocation location, List<Guid> containerGuidList, List<string> containerNames, List<string> containerIconTags)
    {
        ObjectId = objectId;
        Location = location;
        ContainerGuidList = containerGuidList;
        ContainerNames = containerNames;
        ContainerIconTags = containerIconTags;
    }

    public long? ObjectId { get; set; }
    public VaultLocation Location { get; set; }
    public List<Guid> ContainerGuidList { get; set; }
    public List<string> ContainerNames { get; set; }
    public List<string> ContainerIconTags { get; set; }
}