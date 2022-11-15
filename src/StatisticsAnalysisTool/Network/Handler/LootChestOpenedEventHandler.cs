using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class LootChestOpenedEventHandler
    {
        private readonly TrackingController _trackingController;

        public LootChestOpenedEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(LootChestOpenedEvent value)
        {
            _trackingController.DungeonController?.SetDungeonChestOpen(value.Id);
            await Task.CompletedTask;
        }
    }
}