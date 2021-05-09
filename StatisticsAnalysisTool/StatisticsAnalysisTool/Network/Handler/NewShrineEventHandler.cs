using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewShrineEventHandler : EventPacketHandler<NewShrineEvent>
    {
        private readonly TrackingController _trackingController;

        public NewShrineEventHandler(TrackingController trackingController) : base((int) EventCodes.NewShrine)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(NewShrineEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.NewShrine, JsonConvert.SerializeObject(value));

            await Task.CompletedTask;
        }
    }
}