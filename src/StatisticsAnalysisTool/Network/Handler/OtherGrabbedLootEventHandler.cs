using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class OtherGrabbedLootEventHandler
    {
        private readonly TrackingController _trackingController;

        public OtherGrabbedLootEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(GrabbedLootEvent value)
        {
            await _trackingController.LootController.AddLootAsync(value.Loot);
        }
    }
}