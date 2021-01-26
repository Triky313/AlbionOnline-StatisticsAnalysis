using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler : EventPacketHandler<TestEvent>
    {
        public TestHandler() : base(79) { }

        protected override async Task OnActionAsync(TestEvent value)
        {
            Debug.Print($"TestHandler");

            await Task.CompletedTask;
        }
    }
}