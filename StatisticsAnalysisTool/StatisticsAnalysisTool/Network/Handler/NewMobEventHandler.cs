using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
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
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.NewMob, JsonConvert.SerializeObject(value));

            //if (value.ObjectId != null)
            //{
            //    _trackingController.EntityController.AddEntity((long) value.ObjectId, string.Empty, GameObjectType.Mob, (GameObjectSubType) value.Type);
            //}
            await Task.CompletedTask;
        }
    }
}