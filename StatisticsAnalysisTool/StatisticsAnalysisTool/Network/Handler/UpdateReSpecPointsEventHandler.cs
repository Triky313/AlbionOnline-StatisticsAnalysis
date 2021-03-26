using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;

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
            _reSpecPointsCountUpTimer.Add(value.CurrentReSpecPoints);

            _trackingController.SetTotalPlayerReSpecPoints(value.CurrentReSpecPoints);
            _trackingController.DungeonController?.AddValueToDungeon(value.CurrentReSpecPoints, ValueType.ReSpec);
            await Task.CompletedTask;
        }
    }
}