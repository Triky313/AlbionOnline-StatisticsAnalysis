using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartySilverGainedEventHandler : EventPacketHandler<PartySilverGainedEvent>
    {
        private readonly TrackingController _trackingController;

        public PartySilverGainedEventHandler(TrackingController trackingController) : base((int) EventCodes.PartySilverGained)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(PartySilverGainedEvent value)
        {
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.PartySilverGained, JsonConvert.SerializeObject(value));

            //Debug.Print($"Total Collected Silver: {value.TotalCollectedSilver}");
            //Debug.Print($"Guild Tax: {value.GuildTax}");
            //Debug.Print($"Earned Silver: {value.EarnedSilver}");

            await Task.CompletedTask;
        }
    }
}