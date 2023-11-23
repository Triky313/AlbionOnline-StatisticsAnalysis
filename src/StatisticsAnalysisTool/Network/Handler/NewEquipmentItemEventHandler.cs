using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewEquipmentItemEventHandler : EventPacketHandler<NewEquipmentItemEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public NewEquipmentItemEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.NewEquipmentItem)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(NewEquipmentItemEvent value)
    {
        if (_gameEventWrapper.TrackingController.IsTrackingAllowedByMainCharacter())
        {
            _gameEventWrapper.VaultController.Add(value.Item);
        }

        _gameEventWrapper.EntityController.AddEquipmentItem(new EquipmentItemInternal
        {
            ItemIndex = value.Item.ItemIndex,
            SpellDictionary = value.Item.SpellDictionary
        });

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);

        _gameEventWrapper.LootController.AddDiscoveredItem(value.Item);
        _gameEventWrapper.DungeonController.AddDiscoveredItem(value.Item);
        _gameEventWrapper.GatheringController.AddFishedItem(value.Item);
        await Task.CompletedTask;
    }
}