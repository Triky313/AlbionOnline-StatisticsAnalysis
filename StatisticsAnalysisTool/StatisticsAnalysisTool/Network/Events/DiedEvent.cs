using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class DiedEvent : BaseEvent
    {
        public DiedEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                foreach (var parameter in parameters)
                {
                    Debug.Print($"{parameter}");
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}