using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TakeSilverEventHandler : EventPacketHandler<TakeSilverEvent>
    {
        public TakeSilverEventHandler() : base((int) EventCodes.TakeSilver) { }

        protected override async Task OnActionAsync(TakeSilverEvent value)
        {
            //Debug.Print($"TakeSilver");
            //Debug.Print($"Total Collected Silver: {value.TotalCollectedSilver}");
            //Debug.Print($"Guild Tax: {value.GuildTax}");
            //Debug.Print($"Earned Silver: {value.EarnedSilver}");

            await Task.CompletedTask;
        }
    }
}