using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler3 : EventPacketHandler<TestEvent3>
    {
        public TestHandler3() : base(365) { }

        protected override async Task OnActionAsync(TestEvent3 value)
        {
            await Task.CompletedTask;
        }
    }
}