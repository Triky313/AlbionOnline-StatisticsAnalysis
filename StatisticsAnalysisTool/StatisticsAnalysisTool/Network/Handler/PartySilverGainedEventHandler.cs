using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartySilverGainedEventHandler : EventPacketHandler<PartySilverGainedEvent>
    {
        public PartySilverGainedEventHandler() : base((int) EventCodes.PartySilverGained)
        {
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