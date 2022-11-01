using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewMobEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewMobEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewMobEvent value)
        {
            //if (value.ObjectId != null)
            //{
            //    TrackingController.EntityController.AddEntity((long) value.ObjectId, string.Empty, GameObjectType.Mob, (GameObjectSubType) value.Type);
            //}
            await Task.CompletedTask;
        }
    }
}