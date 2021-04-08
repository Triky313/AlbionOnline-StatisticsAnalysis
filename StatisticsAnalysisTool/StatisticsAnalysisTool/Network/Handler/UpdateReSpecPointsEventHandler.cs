using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateReSpecPointsEventHandler : EventPacketHandler<UpdateReSpecPointsEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly CountUpTimer _countUpTimer;

        public UpdateReSpecPointsEventHandler(TrackingController trackingController) : base(
            (int) EventCodes.UpdateReSpecPoints)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateReSpecPointsEvent value)
        {
            if (value?.CurrentReSpecPoints != null)
            {
                _countUpTimer.Add(ValueType.ReSpec, value.CurrentReSpecPoints.Value.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.CurrentReSpecPoints.Value.DoubleValue, ValueType.ReSpec);
            }
            
            await Task.CompletedTask;
        }
    }
}