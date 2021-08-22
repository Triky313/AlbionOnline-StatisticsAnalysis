using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
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
            //Debug.Print($"Total Collected Silver: {value.TotalCollectedSilver}");
            //Debug.Print($"Guild Tax: {value.GuildTax}");
            //Debug.Print($"Earned Silver: {value.EarnedSilver}");

            await Task.CompletedTask;
        }
    }
}