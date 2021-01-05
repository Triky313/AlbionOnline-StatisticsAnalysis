using Albion.Network;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TakeSilverEventHandler : EventPacketHandler<TakeSilverEvent>
    {
        public TakeSilverEventHandler() : base(EventCodes.TakeSilver) { }

        protected override async Task OnActionAsync(TakeSilverEvent value)
        {
            Debug.Print($"TakeSilver");
            Debug.Print($"UserId: {value.UserId}");
            Debug.Print($"Total Collected Silver: {value.TotalCollectedSilver}");
            Debug.Print($"Guild Tax: {value.GuildTax}");
            Debug.Print($"Earned Silver: {value.EarnedSilver}");

            EventCounter();
        }

        private long eventCounter;

        private void EventCounter()
        {
            eventCounter++;
            Debug.Print($"Event counter: {eventCounter}");
        }
    }
}