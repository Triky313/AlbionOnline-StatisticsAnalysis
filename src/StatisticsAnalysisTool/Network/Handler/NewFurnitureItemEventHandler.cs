using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.EstimatedMarketValue;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewFurnitureItemEventHandler : EventPacketHandler<NewFurnitureItemEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public NewFurnitureItemEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.NewFurnitureItem)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(NewFurnitureItemEvent value)
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