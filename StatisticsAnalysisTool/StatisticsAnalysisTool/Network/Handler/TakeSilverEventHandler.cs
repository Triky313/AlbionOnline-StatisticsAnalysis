using System.Threading.Tasks;
using Albion.Network;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TakeSilverEventHandler : EventPacketHandler<TakeSilverEvent>
    {
        public TakeSilverEventHandler() : base((int) EventCodes.TakeSilver)
        {
        }

        protected override async Task OnActionAsync(TakeSilverEvent value)
        {
            await Task.CompletedTask;
        }
    }
}