using StatisticsAnalysisTool.EstimatedMarketValue;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewFurnitureItemEventHandler(TrackingController trackingController) : EventPacketHandler<NewFurnitureItemEvent>((int) EventCodes.NewFurnitureItem)
{
    protected override async Task OnActionAsync(NewFurnitureItemEvent value)
    {


        if (trackingController.IsTrackingAllowedByMainCharacter())
        {
            trackingController.VaultController.AddDiscoveredItem(value.Item);
        }

        EstimatedMarketValueController.Add(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal, value.Item.Quality);
        trackingController.LootController.AddDiscoveredItem(value.Item);
        trackingController.DungeonController.AddDiscoveredItem(value.Item);
        trackingController.GatheringController.AddFishedItem(value.Item);
        await Task.CompletedTask;
    }
}