using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewKillTrophyItemHandler : EventPacketHandler<NewKillTrophyItemEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public NewKillTrophyItemHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.NewKillTrophyItem)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(NewKillTrophyItemEvent value)
    {
        if (_gameEventWrapper.TrackingController.IsTrackingAllowedByMainCharacter())
        {
            _gameEventWrapper.VaultController.Add(value.Item);
        }

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);
        _gameEventWrapper.LootController.AddDiscoveredItem(value.Item);
        _gameEventWrapper.DungeonController.AddDiscoveredItem(value.Item);
        await Task.CompletedTask;
    }
}