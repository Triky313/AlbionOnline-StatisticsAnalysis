using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler2 : EventPacketHandler<TestEvent2>
    {
        public TestHandler2() : base(115) { }

        protected override async Task OnActionAsync(TestEvent2 value)
        {
            await Task.CompletedTask;
        }
    }
}