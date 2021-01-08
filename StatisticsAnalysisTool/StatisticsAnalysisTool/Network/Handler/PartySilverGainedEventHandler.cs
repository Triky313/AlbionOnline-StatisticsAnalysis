using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartySilverGainedEventHandler : EventPacketHandler<PartySilverGainedEvent>
    {
        public PartySilverGainedEventHandler() : base(EventCodes.PartySilverGained) { }

        protected override async Task OnActionAsync(PartySilverGainedEvent value)
        {
            Debug.Print($"PartySilverGained");
            //Debug.Print($"Total Collected Silver: {value.TotalCollectedSilver}");
            //Debug.Print($"Guild Tax: {value.GuildTax}");
            //Debug.Print($"Earned Silver: {value.EarnedSilver}");

            await Task.CompletedTask;
        }
    }
}