using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler : ResponsePacketHandler<TestEvent>
    {
        public TestHandler() : base(280) { }

        protected override async Task OnActionAsync(TestEvent value)
        {
            Debug.Print($"----- TestHandler");
            await Task.CompletedTask;
        }
    }
}