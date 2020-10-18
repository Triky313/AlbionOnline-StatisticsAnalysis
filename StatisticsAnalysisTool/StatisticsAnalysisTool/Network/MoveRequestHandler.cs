using Albion.Network;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network
{
    public class MoveRequestHandler : RequestPacketHandler<MoveOperation>
    {
        public MoveRequestHandler() : base(OperationCodes.Move) { }

        protected override void OnAction(MoveOperation value)
        {
            Debug.Print($"Move request");
        }
    }
}