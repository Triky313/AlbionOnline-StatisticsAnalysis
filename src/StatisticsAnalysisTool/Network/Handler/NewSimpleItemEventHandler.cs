using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewSimpleItemEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewSimpleItemEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewSimpleItemEvent value)
        {
            if (_trackingController.IsTrackingAllowedByMainCharacter())
            {
                _trackingController.VaultController.Add(value.Item);
            }

            _trackingController.LootController.AddEstimatedMarketValue(value.Item.ItemIndex, value.Item.EstimatedMarketValue.InternalValue);
            await Task.CompletedTask;
        }
    }
}