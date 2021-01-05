using Albion.Network;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewLootChestEvent : BaseEvent
    {
        public NewLootChestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                Debug.Print($"{parameters[0]} {parameters[1]} {parameters[2]} {parameters[3]} {parameters[4]} {parameters[5]} {parameters[6]}");
            }
            catch
            {
            }
        }

        public string ContainerId { get; }
        public string ContainerName { get; }
    }
}