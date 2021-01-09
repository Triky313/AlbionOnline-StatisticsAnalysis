using Albion.Network;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network
{
    public class NewRandomDungeonExitEvent : BaseEvent
    {
        public NewRandomDungeonExitEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print($"-----------------------------------------");
            Debug.Print($"NewRandomDungeonExit");

            foreach (var parameter in parameters)
            {
                Debug.Print($"{parameter}");
            }
        }
    }
}