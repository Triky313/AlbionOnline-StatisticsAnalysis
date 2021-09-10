using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
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
            await _trackingController.LootController.AddLootAsync(value.Loot);
        }
    }
}