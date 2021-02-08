using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class TestEvent2 : BaseEvent
    {
        public TestEvent2(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print($"----- NewLoot -----");

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