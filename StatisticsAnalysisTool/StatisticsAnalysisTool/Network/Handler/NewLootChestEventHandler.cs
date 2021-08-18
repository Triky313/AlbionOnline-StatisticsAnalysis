using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

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
            _trackingController.DungeonController?.SetDungeonEventObjectInformation(value.Id, value.UniqueName);
            await Task.CompletedTask;
        }
    }
}