using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewMobEventHandler : EventPacketHandler<NewMobEvent>
    {
        private readonly TrackingController _trackingController;

        public NewMobEventHandler(TrackingController trackingController) : base((int) EventCodes.NewMob)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(NewMobEvent value)
        {
            //if (value.ObjectId != null)
            //{
            //    TrackingController.EntityController.AddEntity((long) value.ObjectId, string.Empty, GameObjectType.Mob, (GameObjectSubType) value.Type);
            //}
            await Task.CompletedTask;
        }
    }
}