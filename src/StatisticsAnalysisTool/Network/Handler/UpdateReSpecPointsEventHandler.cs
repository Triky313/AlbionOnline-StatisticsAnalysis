using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateReSpecPointsEventHandler
    {
        private readonly TrackingController _trackingController;
        private readonly CountUpTimer _countUpTimer;

        public UpdateReSpecPointsEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController?.CountUpTimer;
        }

        public async Task OnActionAsync(UpdateReSpecPointsEvent value)
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