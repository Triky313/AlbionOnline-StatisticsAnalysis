using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class LeaveEventHandler : EventPacketHandler<LeaveEvent>
    {
        private readonly TrackingController _trackingController;

        public LeaveEventHandler(TrackingController trackingController) : base((int) EventCodes.NewMob)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(LeaveEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.NewMob, JsonConvert.SerializeObject(value));

            //if (value.ObjectId != null)
            //{
            //    _trackingController.EntityController.RemoveEntity((long)value.ObjectId);
            //}
            await Task.CompletedTask;
        }
    }
}