using Albion.Network;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network
{
    public class NewCharacterEventHandler : EventPacketHandler<NewCharacterEvent>
    {
        public NewCharacterEventHandler() : base(EventCodes.NewCharacter) { }

        protected override void OnAction(NewCharacterEvent value)
        {
            Debug.Print($"New ch Name: {value.Name}");
        }
    }
}