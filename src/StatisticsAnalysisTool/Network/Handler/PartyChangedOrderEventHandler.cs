using StatisticsAnalysisTool.Network.Manager;
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
            await _trackingController.EntityController.SetPartyAsync(value.PartyUsersGuid, true);
        }
    }
}