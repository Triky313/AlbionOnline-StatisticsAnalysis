using Albion.Network;
using StatisticsAnalysisTool.Network.Events;
using System.Diagnostics;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class TestHandler2 : RequestPacketHandler<TestEvent>
    {
        public TestHandler2() : base(170) { }

        protected override async Task OnActionAsync(TestEvent value)
        {
            Debug.Print($"----- TestHandler 2");
            await Task.CompletedTask;
        }
    }
}