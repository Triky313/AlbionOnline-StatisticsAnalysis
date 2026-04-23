using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.StorageHistory;

namespace StatisticsAnalysisTool.Network.Events;

public class GuildVaultInfoEvent
{
    public long? ObjectId;
    public string LocationGuidString;
    public List<Guid> VaultGuidList = [];
    public List<string> VaultNames = [];
    public List<string> IconTags = [];

    public GuildVaultInfoEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object locationGuid))
            {
                LocationGuidString = locationGuid.ToString();
            }

            if (parameters.TryGetValue(2, out object vaultGuids))
            {
                VaultGuidList = VaultInfoParser.GetGuids(vaultGuids);
            }

            if (parameters.TryGetValue(3, out object vaultNames))
            {
                VaultNames = VaultInfoParser.GetStrings(vaultNames);
            }

            if (parameters.TryGetValue(4, out object iconTags))
            {
                IconTags = VaultInfoParser.GetStrings(iconTags);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}
