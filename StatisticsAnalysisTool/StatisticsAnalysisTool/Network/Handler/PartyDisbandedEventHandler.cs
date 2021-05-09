using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyDisbandedEventHandler : EventPacketHandler<PartyDisbandedEvent>
    {
        private readonly TrackingController _trackingController;

        public PartyDisbandedEventHandler(TrackingController trackingController) : base((int) EventCodes.PartyDisbanded)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(PartyDisbandedEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.PartyDisbanded, JsonConvert.SerializeObject(value));

            _trackingController.EntityController.SetParty(value.PartyUsersGuid, true);
            await Task.CompletedTask;
        }
    }
}