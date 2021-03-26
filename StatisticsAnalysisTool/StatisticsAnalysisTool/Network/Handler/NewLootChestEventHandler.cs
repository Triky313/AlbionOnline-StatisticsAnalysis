using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewLootChestEventHandler : EventPacketHandler<NewLootChestEvent>
    {
        private readonly TrackingController _trackingController;

        public NewLootChestEventHandler(TrackingController trackingController) : base((int) EventCodes.NewLootChest)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(NewLootChestEvent value)
        {
            _trackingController.DungeonController?.SetDungeonChestInformation(value.Id, value.UniqueName);
            await Task.CompletedTask;
        }
    }
}