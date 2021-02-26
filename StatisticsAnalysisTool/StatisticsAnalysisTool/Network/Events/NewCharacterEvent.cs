using Albion.Network;
using StatisticsAnalysisTool.Common;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewCharacterEvent : BaseEvent
    {
        public NewCharacterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print($"--- NewCharacter (Event) ---");
            //foreach (var parameter in parameters)
            //{
            //    Debug.Print($"{parameter}");
            //}

            if (parameters.ContainsKey(0))
            {
                ObjectId = parameters[0].ObjectToLong();
            }

            if (parameters.ContainsKey(1))
            {
                Name = parameters[1].ToString();
            }

            if (parameters.ContainsKey(8))
            {
                GuildName = parameters[8].ToString();
            }

            if (parameters.ContainsKey(12))
            {
                Position = (float[])parameters[12];
            }

            Debug.Print($"ObjectId: {ObjectId}");
            Debug.Print($"Name: {Name}");
            Debug.Print($"GuildName: {GuildName}");
        }

        public long? ObjectId { get; }
        public string Name { get; }
        public string GuildName { get; }
        public float[] Position { get; }
    }
}