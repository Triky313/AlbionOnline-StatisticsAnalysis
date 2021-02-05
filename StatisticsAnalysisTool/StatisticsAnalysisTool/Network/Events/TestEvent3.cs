using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class TestEvent3 : BaseEvent
    {
        public TestEvent3(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print($"----- LootChestOpened -----");

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