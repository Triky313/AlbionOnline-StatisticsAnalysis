using System;
using System.Collections.Generic;
using System.Diagnostics;
using Albion.Network;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events
{
    public class NewCharacterEvent : BaseEvent
    {
        public NewCharacterEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0)) ObjectId = parameters[0].ObjectToLong();

                if (parameters.ContainsKey(1)) Name = parameters[1].ToString();

                if (parameters.ContainsKey(7)) Guid = parameters[7].ObjectToGuid();

                if (parameters.ContainsKey(8)) GuildName = parameters[8].ToString();

                if (parameters.ContainsKey(12)) Position = (float[]) parameters[12];
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public long? ObjectId { get; }
        public Guid? Guid { get; }
        public string Name { get; }
        public string GuildName { get; }
        public float[] Position { get; }
    }
}