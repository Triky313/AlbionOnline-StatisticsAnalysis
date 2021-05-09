using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

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
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.LootChestOpened, JsonConvert.SerializeObject(value));

            _trackingController.DungeonController?.SetDungeonChestOpen(value.Id);
            await Task.CompletedTask;
        }
    }
}