using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewCharacterEventHandler : EventPacketHandler<NewCharacterEvent>
    {
        public NewCharacterEventHandler() : base((int) EventCodes.NewCharacter) { }

        protected override async Task OnActionAsync(NewCharacterEvent value)
        {
            await Task.CompletedTask;
        }
    }
}