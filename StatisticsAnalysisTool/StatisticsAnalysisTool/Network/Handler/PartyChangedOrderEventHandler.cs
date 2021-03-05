using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyChangedOrderEventHandler : EventPacketHandler<PartyChangedOrderEvent>
    {
        private readonly TrackingController _trackingController;
        public PartyChangedOrderEventHandler(TrackingController trackingController) : base((int) EventCodes.PartyChangedOrder)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(PartyChangedOrderEvent value)
        {
            _trackingController.EntityController.SetInParty(value.Username);
            await Task.CompletedTask;
        }
    }
}