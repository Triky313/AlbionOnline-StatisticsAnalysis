using System;
using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;

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
            if (value?.UserGuid != null) _trackingController.EntityController.AddToParty((Guid) value.UserGuid, value.Username);
            await Task.CompletedTask;
        }
    }
}