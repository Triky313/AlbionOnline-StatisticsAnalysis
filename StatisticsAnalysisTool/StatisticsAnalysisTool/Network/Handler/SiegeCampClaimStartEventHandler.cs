using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    /// <summary>
    ///     Triggered when silver is picked up. Each party member gets their own event.
    /// </summary>
    public class SiegeCampClaimStartEventHandler : EventPacketHandler<SiegeCampClaimStartEvent>
    {
        private readonly TrackingController _trackingController;

        public SiegeCampClaimStartEventHandler(TrackingController trackingController) : base((int) EventCodes.SiegeCampClaimStart)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(SiegeCampClaimStartEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.SiegeCampClaimStart, JsonConvert.SerializeObject(value));

            await Task.CompletedTask;
        }
    }
}