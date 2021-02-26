using Albion.Network;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class LootChestOpenedEventHandler : EventPacketHandler<LootChestOpenedEvent>
    {
        private readonly TrackingController _trackingController;
        public LootChestOpenedEventHandler(TrackingController trackingController) : base(365)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(LootChestOpenedEvent value)
        {
            _trackingController.SetDungeonChestOpen(value.Id);
            await Task.CompletedTask;
        }
    }
}