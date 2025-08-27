using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class PartyJoinedEvent
{
    public readonly Dictionary<Guid, string> PartyUsers = new();

    public PartyJoinedEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0) && parameters[0] != null)
            {
                var partyUsersByteArrays = ((object[]) parameters[4]).ToDictionary();
                var partyUserNameArray = ((string[]) parameters[5]).ToDictionary();

                for (var i = 0; i < partyUsersByteArrays.Count; i++)
                {
                    var guid = partyUsersByteArrays[i].ObjectToGuid();
                    var name = partyUserNameArray[i];
                    if (guid != null && !string.IsNullOrEmpty(name))
                    {
                        PartyUsers.Add((Guid) guid, name);
                    }
                }
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}