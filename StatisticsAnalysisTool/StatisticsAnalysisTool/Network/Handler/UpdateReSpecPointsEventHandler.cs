using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateReSpecPointsEventHandler : EventPacketHandler<UpdateReSpecPointsEvent>
    {
        private readonly ReSpecPointsCountUpTimer _reSpecPointsCountUpTimer;
        private readonly TrackingController _trackingController;

        public UpdateReSpecPointsEventHandler(TrackingController trackingController, ReSpecPointsCountUpTimer reSpecPointsCountUpTimer) : base(
            (int) EventCodes.UpdateReSpecPoints)
        {
            _trackingController = trackingController;
            _reSpecPointsCountUpTimer = reSpecPointsCountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateReSpecPointsEvent value)
        {
            if (value?.CurrentReSpecPoints != null)
            {
                _reSpecPointsCountUpTimer.Add(value.CurrentReSpecPoints.Value.DoubleValue);

                _trackingController.SetTotalPlayerReSpecPoints(value.CurrentReSpecPoints.Value.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.CurrentReSpecPoints.Value.DoubleValue, ValueType.ReSpec);
            }
            
            await Task.CompletedTask;
        }
    }
}