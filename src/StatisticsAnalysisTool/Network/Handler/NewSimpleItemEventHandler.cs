using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewSimpleItemEventHandler : EventPacketHandler<NewSimpleItemEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public NewSimpleItemEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.NewSimpleItem)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(NewSimpleItemEvent value)
    {
        if (_gameEventWrapper.TrackingController.IsTrackingAllowedByMainCharacter())
        {
            _gameEventWrapper.VaultController.Add(value.Item);
        }

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);
        _gameEventWrapper.LootController.AddDiscoveredItem(value.Item);
        _gameEventWrapper.DungeonController.AddDiscoveredItem(value.Item);
        _gameEventWrapper.GatheringController.AddFishedItem(value.Item);
        await Task.CompletedTask;
    }
}