using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class SiegeCampClaimStartEvent : BaseEvent
    {
        public SiegeCampClaimStartEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(2))
                {
                    Username = string.IsNullOrEmpty(parameters[2].ToString()) ? string.Empty : parameters[2].ToString();
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }
        
        public string Username;
    }
}