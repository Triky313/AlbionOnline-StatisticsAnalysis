using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyDisbandedEvent : BaseEvent
    {
        public Dictionary<Guid, string> PartyUsersGuid = new Dictionary<Guid, string>();

        public PartyDisbandedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

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
                        if (guid != null && !string.IsNullOrEmpty(name)) PartyUsersGuid.Add((Guid) guid, name);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}