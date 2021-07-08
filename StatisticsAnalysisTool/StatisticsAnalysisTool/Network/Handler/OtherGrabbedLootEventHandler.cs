using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class OtherGrabbedLootEventHandler : EventPacketHandler<OtherGrabbedLootEvent>
    {
        private readonly TrackingController _trackingController;

        public OtherGrabbedLootEventHandler(TrackingController trackingController) : base((int) EventCodes.OtherGrabbedLoot)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(OtherGrabbedLootEvent value)
        {
            _trackingController.LootController.AddLoot(value.Loot);
            await Task.CompletedTask;
        }
    }
}