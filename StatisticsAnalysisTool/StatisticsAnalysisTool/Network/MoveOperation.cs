using Albion.Network;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network
{
    public class MoveOperation : BaseOperation
    {
        public MoveOperation(Dictionary<byte, object> parameters) : base(parameters)
        {
            Time = (int)parameters[0];
            Position = (float[])parameters[1];
            Direction = (float)parameters[2];
            NewPosition = (float[])parameters[3];
            Speed = (float)parameters[4];
        }

        public int Time { get; }
        public float[] Position { get; }
        public float Direction { get; }
        public float[] NewPosition { get; }
        public float Speed { get; }
    }
}