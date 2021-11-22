using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
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
            //_trackingController.LootController.AddDiscoveredLoot(value.Loot);
            await Task.CompletedTask;
        }
    }
}