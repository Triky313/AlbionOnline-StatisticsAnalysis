using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class HealthUpdateEventHandler
    {
        private readonly TrackingController _trackingController;

        public HealthUpdateEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(HealthUpdateEvent value)
        {
            _trackingController.EntityController.HealthUpdate(value.ObjectId, value.TimeStamp, value.HealthChange, value.NewHealthValue,
                value.EffectType, value.EffectOrigin, value.CauserId, value.CausingSpellType);

            await Task.CompletedTask;
        }
    }
}