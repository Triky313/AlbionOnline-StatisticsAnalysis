using System;
using System.Collections.Generic;
using System.Diagnostics;
using Albion.Network;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events
{
    public class LeaveEvent : BaseEvent
    {
        public LeaveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0)) ObjectId = parameters[0].ObjectToLong();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public long? ObjectId { get; }
    }
}