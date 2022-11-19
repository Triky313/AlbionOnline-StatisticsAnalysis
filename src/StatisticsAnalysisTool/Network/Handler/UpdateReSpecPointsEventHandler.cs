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
                _liveStatsTracker.Add(ValueType.ReSpec, value.GainedReSpecPoints.DoubleValue);
                _liveStatsTracker.Add(ValueType.PaidSilverForReSpec, value.PaidSilver.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.GainedReSpecPoints.DoubleValue, ValueType.ReSpec);
                _trackingController.StatisticController?.AddValue(ValueType.ReSpec, value.GainedReSpecPoints.DoubleValue);
            }

            await Task.CompletedTask;
        }
    }
}