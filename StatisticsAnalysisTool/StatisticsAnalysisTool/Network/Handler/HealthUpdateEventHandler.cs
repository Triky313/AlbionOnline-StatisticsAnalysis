using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class HealthUpdateEventHandler : EventPacketHandler<HealthUpdateEvent>
    {
        private readonly TrackingController _trackingController;

        public HealthUpdateEventHandler(TrackingController trackingController) : base((int) EventCodes.HealthUpdate)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(HealthUpdateEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.HealthUpdate, JsonConvert.SerializeObject(value));

            _trackingController.EntityController.HealthUpdate(value.ObjectId, value.TimeStamp, value.HealthChange, value.NewHealthValue,
                value.EffectType, value.EffectOrigin, value.CauserId, value.CausingSpellType);

            await Task.CompletedTask;
        }
    }
}