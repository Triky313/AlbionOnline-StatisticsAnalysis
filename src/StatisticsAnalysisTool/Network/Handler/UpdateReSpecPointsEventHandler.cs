using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateReSpecPointsEventHandler : EventPacketHandler<UpdateReSpecPointsEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public UpdateReSpecPointsEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.UpdateReSpecPoints)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(UpdateReSpecPointsEvent value)
    {
        if (value?.CurrentTotalReSpecPoints != null)
        {
            _gameEventWrapper.LiveStatsTracker.Add(ValueType.ReSpec, value.GainedReSpecPoints.DoubleValue);
            _gameEventWrapper.LiveStatsTracker.Add(ValueType.PaidSilverForReSpec, value.PaidSilver.DoubleValue);
            _gameEventWrapper.DungeonController?.AddValueToDungeon(value.GainedReSpecPoints.DoubleValue, ValueType.ReSpec);
            _gameEventWrapper.StatisticController?.AddValue(ValueType.ReSpec, value.GainedReSpecPoints.DoubleValue);
        }

        await Task.CompletedTask;
    }
}