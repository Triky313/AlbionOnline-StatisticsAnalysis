using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
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
            if (value.ObjectId != null)
            {
                _trackingController.EntityController.RemoveEntity((long)value.ObjectId);
            }
            await Task.CompletedTask;
        }
    }
}