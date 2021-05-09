using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TakeSilverEventHandler : EventPacketHandler<TakeSilverEvent>
    {
        private readonly TrackingController _trackingController;

        public TakeSilverEventHandler(TrackingController trackingController) : base((int) EventCodes.TakeSilver)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(TakeSilverEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.TakeSilver, JsonConvert.SerializeObject(value));

            var localEntity = _trackingController.EntityController.GetLocalEntity()?.Value;
            if (localEntity?.ObjectId == value.ObjectId)
            {
                _trackingController.CountUpTimer.Add(ValueType.Silver, value.YieldAfterTax.DoubleValue);
                _trackingController.DungeonController?.AddValueToDungeon(value.YieldAfterTax.DoubleValue, ValueType.Silver);
            }

            await Task.CompletedTask;
        }
    }
}