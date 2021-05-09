using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
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
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.NewLootChest, JsonConvert.SerializeObject(value));

            _trackingController.DungeonController?.SetDungeonChestInformation(value.Id, value.UniqueName);
            await Task.CompletedTask;
        }
    }
}