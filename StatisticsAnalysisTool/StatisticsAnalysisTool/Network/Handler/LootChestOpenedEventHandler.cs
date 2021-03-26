using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class LootChestOpenedEventHandler : EventPacketHandler<LootChestOpenedEvent>
    {
        private readonly TrackingController _trackingController;

        public LootChestOpenedEventHandler(TrackingController trackingController) : base((int) EventCodes.LootChestOpened)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(LootChestOpenedEvent value)
        {
            _trackingController.DungeonController?.SetDungeonChestOpen(value.Id);
            await Task.CompletedTask;
        }
    }
}