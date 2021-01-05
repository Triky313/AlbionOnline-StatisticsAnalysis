using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network
{
    public class TakeSilverEvent : BaseEvent
    {
        public TakeSilverEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                //Debug.Print($"{parameters[0]}");
                //Debug.Print($"{parameters[1]}");
                //Debug.Print($"{parameters[2]}");
                //Debug.Print($"{parameters[3]}");

                TotalCollectedSilver = int.Parse(parameters[3].ToString().Remove(parameters[3].ToString().Length - 4)); ;
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public int TotalCollectedSilver { get; }
    }
}