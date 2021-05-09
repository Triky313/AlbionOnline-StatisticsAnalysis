using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class InCombatStateUpdateEventHandler : EventPacketHandler<InCombatStateUpdateEvent>
    {
        private readonly TrackingController _trackingController;

        public InCombatStateUpdateEventHandler(TrackingController trackingController) : base((int) EventCodes.InCombatStateUpdate)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(InCombatStateUpdateEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.InCombatStateUpdate, JsonConvert.SerializeObject(value));

            if (value.ObjectId != null)
            {
                _trackingController.CombatController.UpdateCombatMode((long) value.ObjectId, value.InActiveCombat, value.InPassiveCombat);
            }

            await Task.CompletedTask;
        }
    }
}