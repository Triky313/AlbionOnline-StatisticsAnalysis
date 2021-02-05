using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class LootChestOpenedEvent : BaseEvent
    {
        public LootChestOpenedEvent(Dictionary<byte, object> parameters) : base(parameters)
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