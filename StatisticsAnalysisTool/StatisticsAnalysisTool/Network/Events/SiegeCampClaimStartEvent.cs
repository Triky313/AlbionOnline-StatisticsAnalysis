using System;
using System.Collections.Generic;
using System.Diagnostics;
using Albion.Network;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class SiegeCampClaimStartEvent : BaseEvent
    {
        public string Username;

        public SiegeCampClaimStartEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(2)) Username = string.IsNullOrEmpty(parameters[2].ToString()) ? string.Empty : parameters[2].ToString();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}