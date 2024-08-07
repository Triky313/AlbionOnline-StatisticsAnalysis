using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewEquipmentItemEventHandler : EventPacketHandler<NewEquipmentItemEvent>
{
    private readonly TrackingController _trackingController;

    public NewEquipmentItemEventHandler(TrackingController trackingController) : base((int) EventCodes.NewEquipmentItem)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewEquipmentItemEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.AddDiscoveredItem(value.Item);
        }

        _trackingController.EntityController.AddEquipmentItem(new EquipmentItemInternal
        {
            ItemIndex = value.Item.ItemIndex,
            SpellDictionary = value.Item.SpellDictionary
        });

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);

        _trackingController.LootController.AddDiscoveredItem(value.Item);
        _trackingController.DungeonController.AddDiscoveredItem(value.Item);
        _trackingController.GatheringController.AddFishedItem(value.Item);
        await Task.CompletedTask;
    }
}