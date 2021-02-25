using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class HealthUpdateEventHandler : EventPacketHandler<HealthUpdateEvent>
    {
        private readonly TrackingController _trackingController;
        public HealthUpdateEventHandler(TrackingController trackingController) : base((int) EventCodes.HealthUpdate)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(HealthUpdateEvent value)
        {
            await Task.CompletedTask;
        }
    }
}