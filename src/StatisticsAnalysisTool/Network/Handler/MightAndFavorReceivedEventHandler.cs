using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler;

public class MightAndFavorReceivedEventHandler : EventPacketHandler<MightAndFavorReceivedEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public MightAndFavorReceivedEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.MightAndFavorReceivedEvent)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(MightAndFavorReceivedEvent value)
    {
        if (_gameEventWrapper.TrackingController.IsTrackingAllowedByMainCharacter())
        {
            _gameEventWrapper.StatisticController?.AddValue(ValueType.Might, value.Might.DoubleValue);
            _gameEventWrapper.StatisticController?.AddValue(ValueType.Favor, value.Favor.DoubleValue);
            _gameEventWrapper.LiveStatsTracker.Add(ValueType.Might, value.Might.DoubleValue);
            _gameEventWrapper.LiveStatsTracker.Add(ValueType.Favor, value.Favor.DoubleValue);

            _gameEventWrapper.DungeonController?.AddValueToDungeon(value.Might.DoubleValue, ValueType.Might);
            _gameEventWrapper.DungeonController?.AddValueToDungeon(value.Favor.DoubleValue, ValueType.Favor);
        }

        await Task.CompletedTask;
    }
}