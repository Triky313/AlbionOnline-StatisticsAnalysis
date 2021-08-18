using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyDisbandedEventHandler : EventPacketHandler<PartyDisbandedEvent>
    {
        private readonly TrackingController _trackingController;

        public PartyDisbandedEventHandler(TrackingController trackingController) : base((int) EventCodes.PartyDisbanded)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(PartyDisbandedEvent value)
        {
            _trackingController.EntityController.SetParty(value.PartyUsersGuid, true);
            await Task.CompletedTask;
        }
    }
}