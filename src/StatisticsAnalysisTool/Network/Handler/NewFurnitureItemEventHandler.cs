using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.EstimatedMarketValue;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewFurnitureItemEventHandler : EventPacketHandler<NewFurnitureItemEvent>
{
    private readonly TrackingController _trackingController;

    public NewFurnitureItemEventHandler(TrackingController trackingController) : base((int) EventCodes.NewFurnitureItem)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(NewFurnitureItemEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.AddDiscoveredItem(value.Item);
        }

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);
        _trackingController.LootController.AddDiscoveredItem(value.Item);
        _trackingController.DungeonController.AddDiscoveredItem(value.Item);
        _trackingController.GatheringController.AddFishedItem(value.Item);
        await Task.CompletedTask;
    }
}