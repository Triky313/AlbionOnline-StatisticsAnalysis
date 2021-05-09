using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class DiedEventHandler : EventPacketHandler<DiedEvent>
    {
        private readonly TrackingController _trackingController;

        public DiedEventHandler(TrackingController trackingController) : base((int)EventCodes.Died)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(DiedEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.Died, JsonConvert.SerializeObject(value));

            _trackingController.DungeonController?.SetDiedIfInDungeon(new DiedObject(value.Died, value.KilledBy, value.KilledByGuild));
            await Task.CompletedTask;
        }
    }
}