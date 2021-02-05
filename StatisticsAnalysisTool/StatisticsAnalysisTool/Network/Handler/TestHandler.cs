using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler : ResponsePacketHandler<TestEvent>
    {
        public TestHandler() : base(34) { }

        protected override async Task OnActionAsync(TestEvent value)
        {
            await Task.CompletedTask;
        }
    }
}