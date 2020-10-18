using Albion.Network;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network
{
    public class MoveEvent : BaseEvent
    {
        public MoveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            var bytes = (byte[])parameters[1];

            Id = parameters[0].ToString();
            Position = new[] { BitConverter.ToSingle(bytes, 9), BitConverter.ToSingle(bytes, 13) };
        }

        public string Id { get; }
        public float[] Position { get; }
    }
}