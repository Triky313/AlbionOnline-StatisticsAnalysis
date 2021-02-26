using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Time;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class PartySilverGainedEvent : BaseEvent
    {
        public PartySilverGainedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                Debug.Print($"--- PartySilverGained (Event) ---");
                foreach (var parameter in parameters)
                {
                    Debug.Print($"{parameter}");
                }

                if (parameters.ContainsKey(0))
                {
                    TimeStamp = new GameTimeStamp(parameters[0].ObjectToLong()?? 0);
                }

                if (parameters.ContainsKey(1))
                {
                    TargetEntityId = parameters[1].ObjectToLong();
                }

                if (parameters.ContainsKey(2))
                {
                    SilverNet = FixPoint.FromInternalValue(parameters[2].ObjectToLong()?? 0);
                }

                if (parameters.ContainsKey(3))
                {
                    SilverPreTax = FixPoint.FromInternalValue(parameters[3].ObjectToLong()?? 0);
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public GameTimeStamp TimeStamp;
        public long? TargetEntityId;
        public FixPoint SilverNet;
        public FixPoint SilverPreTax;
    }
}