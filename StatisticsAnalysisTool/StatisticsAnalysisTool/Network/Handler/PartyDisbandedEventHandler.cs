using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyDisbandedEventHandler : EventPacketHandler<PartyDisbandedEvent>
    {
        public PartyDisbandedEventHandler() : base((int) EventCodes.PartyDisbanded) { }

        protected override async Task OnActionAsync(PartyDisbandedEvent value)
        {
            await Task.CompletedTask;
        }
    }
}