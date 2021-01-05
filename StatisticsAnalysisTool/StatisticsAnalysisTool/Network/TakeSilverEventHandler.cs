using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network
{
    public class TakeSilverEventHandler : EventPacketHandler<TakeSilverEvent>
    {
        public TakeSilverEventHandler() : base(52) { }

        protected override async Task OnActionAsync(TakeSilverEvent value)
        {
            Debug.Print($"TakeSilver");
            Debug.Print($"Collected Silver: {value.TotalCollectedSilver}");
        }
    }
}