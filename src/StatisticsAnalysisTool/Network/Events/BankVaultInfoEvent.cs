using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class BankVaultInfoEvent
{
    public long? ObjectId;
    public string LocationGuidString;
    public List<Guid> VaultGuidList = new();
    public List<string> VaultNames = new();
    public List<string> IconTags = new();

    public BankVaultInfoEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }

            if (parameters.ContainsKey(1))
            {
                LocationGuidString = parameters[1].ToString();
            }

            if (parameters.ContainsKey(2) && parameters[2] != null)
            {
                var vaultGuidArray = ((object[])parameters[2]).ToDictionary();

                for (var i = 0; i < vaultGuidArray.Count; i++)
                {
                    var guid = vaultGuidArray[i].ObjectToGuid();
                    if (guid != null)
                    {
                        VaultGuidList.Add((Guid)guid);
                    }
                }
            }

            if (parameters.ContainsKey(3) && parameters[3] != null)
            {
                var vaultNameArray = ((object[])parameters[3]).ToDictionary();

                for (var i = 0; i < vaultNameArray.Count; i++)
                {
                    VaultNames.Add(vaultNameArray[i].ToString());
                }
            }

            if (parameters.ContainsKey(4) && parameters[4] != null)
            {
                var iconTagArray = ((object[])parameters[4]).ToDictionary();

                for (var i = 0; i < iconTagArray.Count; i++)
                {
                    IconTags.Add(iconTagArray[i].ToString());
                }
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}