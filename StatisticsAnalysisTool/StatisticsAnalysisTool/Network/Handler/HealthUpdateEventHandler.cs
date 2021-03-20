using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
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
            _trackingController.EntityController.HealthUpdate(value.ObjectId, value.TimeStamp, value.HealthChange, value.NewHealthValue, value.EffectType, value.EffectOrigin, value.CauserId, value.CausingSpellType);
            await Task.CompletedTask;
        }
    }
}