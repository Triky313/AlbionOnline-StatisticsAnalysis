using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UpdateMoneyEventHandler : EventPacketHandler<UpdateMoneyEvent>
    {
        private readonly TrackingController _trackingController;
        private readonly CountUpTimer _countUpTimer;

        public UpdateMoneyEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateMoney)
        {
            _trackingController = trackingController;
            _countUpTimer = _trackingController.CountUpTimer;
        }

        protected override async Task OnActionAsync(UpdateMoneyEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.UpdateMoney, JsonConvert.SerializeObject(value));

            await Task.CompletedTask;
        }
    }
}