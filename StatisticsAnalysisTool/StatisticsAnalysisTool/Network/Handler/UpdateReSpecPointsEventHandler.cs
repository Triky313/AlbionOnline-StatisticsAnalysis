using Albion.Network;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateReSpecPointsEventHandler : EventPacketHandler<UpdateReSpecPointsEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly ReSpecPointsCountUpTimer _reSpecPointsCountUpTimer;

        public UpdateReSpecPointsEventHandler(TrackingController trackingController, ReSpecPointsCountUpTimer reSpecPointsCountUpTimer) : base(EventCodes.UpdateReSpecPoints)
        {
            _trackingController = trackingController;
            _reSpecPointsCountUpTimer = reSpecPointsCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateReSpecPointsEvent value)
        {
            _reSpecPointsCountUpTimer.Add(value.CurrentReSpecPoints);

            _trackingController.SetTotalPlayerReSpecPoints(value.CurrentReSpecPoints);
            await Task.CompletedTask;
        }
    }
}