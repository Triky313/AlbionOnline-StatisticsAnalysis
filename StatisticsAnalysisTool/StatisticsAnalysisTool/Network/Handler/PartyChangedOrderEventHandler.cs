using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
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
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.PartyChangedOrder, JsonConvert.SerializeObject(value));

            if (value?.UserGuid != null) _trackingController.EntityController.AddToParty((Guid) value.UserGuid, value.Username);
            await Task.CompletedTask;
        }
    }
}