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
            if (value?.CurrentTotalReSpecPoints != null)
            {
                _countUpTimer.Add(ValueType.ReSpec, value.CurrentTotalReSpecPoints.Value.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.CurrentTotalReSpecPoints.Value.DoubleValue, ValueType.ReSpec);
                _trackingController.StatisticController?.AddValue(ValueType.ReSpec, value.CurrentTotalReSpecPoints.Value.DoubleValue);
            }

            await Task.CompletedTask;
        }
    }
}