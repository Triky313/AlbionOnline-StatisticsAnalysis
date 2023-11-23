using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewJournalItemEventHandler : EventPacketHandler<NewJournalItemEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public NewJournalItemEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.NewJournalItem)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(NewJournalItemEvent value)
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