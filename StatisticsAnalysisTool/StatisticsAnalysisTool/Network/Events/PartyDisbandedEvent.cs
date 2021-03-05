using Albion.Network;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyDisbandedEvent : BaseEvent
    {
        public PartyDisbandedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                Debug.Print($"--- PartyDisbanded (Event) ---");
                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}

                if (parameters.ContainsKey(0) && parameters[0] != null)
                {
                    var partyUserArray = ((string[])parameters[5]).ToDictionary();

                    if (!partyUserArray.IsNullOrEmpty() && partyUserArray.Count > 0)
                    {
                        foreach (var user in partyUserArray)
                        {
                            PartyUsers.Add(user.Value);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }
        
        public List<string> PartyUsers = new List<string>();
    }
}