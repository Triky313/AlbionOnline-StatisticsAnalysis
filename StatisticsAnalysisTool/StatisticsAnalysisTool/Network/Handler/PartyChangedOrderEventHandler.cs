using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyChangedOrderEventHandler
    {
        private readonly TrackingController _trackingController;

        public PartyChangedOrderEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(PartyChangedOrderEvent value)
        {
            if (value?.UserGuid != null)
            {
                await _trackingController.EntityController.AddToPartyAsync((Guid)value.UserGuid, value.Username);
            }
        }
    }
}