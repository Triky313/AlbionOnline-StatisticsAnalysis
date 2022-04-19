using System;
using System.Collections.Generic;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class Vault
{
    public Vault(long? objectId, VaultLocation location, List<Guid> vaultGuidList, List<string> vaultNames, List<string> iconTags)
    {
        ObjectId = objectId;
        Location = location;
        VaultGuidList = vaultGuidList;
        VaultNames = vaultNames;
        IconTags = iconTags;
    }

    public long? ObjectId { get; set; }
    public VaultLocation Location { get; set; }
    public List<Guid> VaultGuidList { get; set; }
    public List<string> VaultNames { get; set; }
    public List<string> IconTags { get; set; }
}