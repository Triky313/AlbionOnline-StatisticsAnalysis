using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateReSpecPointsEventHandler
    {
        private readonly TrackingController _trackingController;
        private readonly LiveStatsTracker _liveStatsTracker;

        public UpdateReSpecPointsEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
            _liveStatsTracker = _trackingController?.LiveStatsTracker;
        }

        public async Task OnActionAsync(UpdateReSpecPointsEvent value)
        {
            if (value?.CurrentTotalReSpecPoints != null)
            {
                _liveStatsTracker.Add(ValueType.ReSpec, value.CurrentTotalReSpecPoints.Value.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.CurrentTotalReSpecPoints.Value.DoubleValue, ValueType.ReSpec);
                _trackingController.StatisticController?.AddValue(ValueType.ReSpec, value.CurrentTotalReSpecPoints.Value.DoubleValue);
            }

            await Task.CompletedTask;
        }
    }
}