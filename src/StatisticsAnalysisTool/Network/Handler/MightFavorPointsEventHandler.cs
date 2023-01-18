using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class MightFavorPointsEventHandler : EventPacketHandler<MightFavorPointsEvent>
{
    private readonly TrackingController _trackingController;
    private readonly LiveStatsTracker _liveStatsTracker;

    public MightFavorPointsEventHandler(TrackingController trackingController) : base((int) EventCodes.MightFavorPoints)
    {
        _trackingController = trackingController;
        _liveStatsTracker = _trackingController?.LiveStatsTracker;
    }

    protected override async Task OnActionAsync(MightFavorPointsEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.StatisticController?.AddValue(ValueType.Might, value.Might.DoubleValue);
            _trackingController.StatisticController?.AddValue(ValueType.Favor, value.Favor.DoubleValue);
            _liveStatsTracker.Add(ValueType.Might, value.Might.DoubleValue);
            _liveStatsTracker.Add(ValueType.Favor, value.Favor.DoubleValue);

            _trackingController.DungeonController?.AddValueToDungeon(value.Might.DoubleValue, ValueType.Might);
            _trackingController.DungeonController?.AddValueToDungeon(value.Favor.DoubleValue, ValueType.Favor);
        }

        await Task.CompletedTask;
    }
}