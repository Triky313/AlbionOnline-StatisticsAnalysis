using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyPlayerLeftEventHandler
    {
        private readonly TrackingController _trackingController;

        public PartyPlayerLeftEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(PartyPlayerLeftEvent value)
        {
            await _trackingController.EntityController.RemoveFromPartyAsync(value.UserGuid);
        }
    }
}