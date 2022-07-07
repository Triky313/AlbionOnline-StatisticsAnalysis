using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateLootChestEventHandler
    {
        private readonly TrackingController _trackingController;

        public UpdateLootChestEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(UpdateLootChestEvent value)
        {
            _trackingController?.TreasureController?.UpdateTreasure(value.ObjectId, value.PlayerGuid);
            await Task.CompletedTask;
        }
    }
}