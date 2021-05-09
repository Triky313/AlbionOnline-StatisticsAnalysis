using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateReSpecPointsEventHandler : EventPacketHandler<UpdateReSpecPointsEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly CountUpTimer _countUpTimer;

        public UpdateReSpecPointsEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateReSpecPoints)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateReSpecPointsEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.UpdateReSpecPoints, JsonConvert.SerializeObject(value));

            if (value?.CurrentReSpecPoints != null)
            {
                _countUpTimer.Add(ValueType.ReSpec, value.CurrentReSpecPoints.Value.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.CurrentReSpecPoints.Value.DoubleValue, ValueType.ReSpec);
            }
            
            await Task.CompletedTask;
        }
    }
}