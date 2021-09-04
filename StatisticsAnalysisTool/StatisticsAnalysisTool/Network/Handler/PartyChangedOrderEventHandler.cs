using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System;
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
            if (value?.UserGuid != null)
            {
                await _trackingController.EntityController.AddToPartyAsync((Guid)value.UserGuid, value.Username);
            }
        }
    }
}