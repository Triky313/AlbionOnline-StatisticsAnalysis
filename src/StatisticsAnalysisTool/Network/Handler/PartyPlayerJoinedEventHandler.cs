using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyPlayerJoinedEventHandler
    {
        private readonly TrackingController _trackingController;

        public PartyPlayerJoinedEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(PartyPlayerJoinedEvent value)
        {
            if (value?.UserGuid != null)
            {
                await _trackingController.EntityController.AddToPartyAsync((Guid)value.UserGuid, value.Username);
            }
        }
    }
}