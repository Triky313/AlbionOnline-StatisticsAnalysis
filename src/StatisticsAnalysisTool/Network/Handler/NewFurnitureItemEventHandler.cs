using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewFurnitureItemEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewFurnitureItemEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewFurnitureItemEvent value)
        {
            if (_trackingController.IsTrackingAllowedByMainCharacter())
            {
                _trackingController.VaultController.Add(value.Item);
            }

            _trackingController.LootController.AddEstimatedMarketValue(value.Item.ItemIndex, value.Item.EstimatedMarketValueInternal);
            _trackingController.LootController.AddDiscoveredItem(value.Item);
            _trackingController.DungeonController.AddDiscoveredItem(value.Item);
            await Task.CompletedTask;
        }
    }
}