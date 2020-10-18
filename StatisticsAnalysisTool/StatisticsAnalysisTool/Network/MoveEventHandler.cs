using Albion.Network;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network
{
    public class MoveEventHandler : EventPacketHandler<MoveEvent>
    {
        public MoveEventHandler() : base(EventCodes.Move) { }

        protected override void OnAction(MoveEvent value)
        {
            Debug.Print($"Id: {value.Id} x: {value.Position[0]} y: {value.Position[1]}");
        }
    }
}