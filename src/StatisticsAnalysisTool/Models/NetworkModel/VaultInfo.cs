using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class VaultInfo
{
    public VaultInfo(long? objectId, string location, List<Guid> containerGuidList, List<string> containerNames, List<string> containerIconTags)
    {
        ObjectId = objectId;
        Location = location;
        ContainerGuidList = containerGuidList;
        ContainerNames = containerNames;
        ContainerIconTags = containerIconTags;
    }

    public long? ObjectId { get; set; }
    public string Location { get; set; }
    public VaultLocation VaultLocation => VaultController.GetVaultLocation(Location);
    public List<Guid> ContainerGuidList { get; set; }
    public List<string> ContainerNames { get; set; }
    public List<string> ContainerIconTags { get; set; }
}