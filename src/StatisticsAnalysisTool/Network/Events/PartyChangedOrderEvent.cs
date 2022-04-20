using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events
{
    public class PartyChangedOrderEvent
    {
        public Dictionary<Guid, string> PartyUsersGuid = new();

        public PartyChangedOrderEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0) && parameters[0] != null)
                {
                    var partyUsersByteArrays = ((object[])parameters[4]).ToDictionary();
                    var partyUserNameArray = ((string[])parameters[5]).ToDictionary();

                    for (var i = 0; i < partyUsersByteArrays.Count; i++)
                    {
                        var guid = partyUsersByteArrays[i].ObjectToGuid();
                        var name = partyUserNameArray[i];
                        if (guid != null && !string.IsNullOrEmpty(name))
                        {
                            PartyUsersGuid.Add((Guid)guid, name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}