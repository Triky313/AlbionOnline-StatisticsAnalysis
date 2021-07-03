using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewSimpleItemEventHandler : EventPacketHandler<NewSimpleItemEvent>
    {
        private readonly TrackingController _trackingController;

        public NewSimpleItemEventHandler(TrackingController trackingController) : base((int) EventCodes.NewSimpleItem)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(NewSimpleItemEvent value)
        {
            _trackingController.LootController.AddDiscoveredLoot(value.Loot);
            await Task.CompletedTask;
        }
    }
}